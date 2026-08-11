# -*- coding: utf-8 -*-
from odoo import api, fields, models
from odoo.exceptions import ValidationError


class SaleOrderExtend(models.Model):
    _inherit = "sale.order"

    external_guid = fields.Char(
        string="External GUID",
        index=True,
        copy=False,
        readonly=True
    )

    external_create_uid = fields.Many2one(
        'res.users',
        'Usuario Creación',
        readonly=True,
        default=lambda self: self.env.user
    )

    _sql_constraints = [
        (
            'external_guid_unique',
            'unique(external_guid)',
            'El External GUID ya existe en otra orden de venta.'
        ),
    ]

    external_payload = fields.Json(
        string="Datos Externos JSON",
        default=dict,
    )

    @api.model_create_multi
    def create(self, vals_list):
        print("Ejecutando ===================================================:")
        orders = super(SaleOrderExtend, self).create(vals_list)

        for order in orders:
            print("Ejecutando ===================================================:")
            print(order.sale_channel)

            if (
                order.id_referencia and
                order.mobile_sync
            ):
                order._procesos_especiales_externa()

        return orders

    def _safe_json_load(self, value):
        import json
        if not value:
            return []
        if isinstance(value, (list, dict)):
            return value if isinstance(value, list) else [value]
        try:
            return json.loads(value)
        except Exception:
            return []

    # =========================================================================
    # ORQUESTADOR PRINCIPAL
    # =========================================================================
    def _procesos_especiales_externa(self):
        for order in self:
            print("==============================================")
            print("Procesando orden:", order.id)

            payload = order.external_payload or {}
            sale_order = payload.get("data", {}).get("sale_order", {})
            order_lines = sale_order.get("order_line", [])

            if not isinstance(order_lines, list):
                continue

            # 0. Mapeo de líneas existentes en el ERP
            existing_lines_map = order._build_existing_lines_map()
            wizard_lines_map = {}

            # 1 Descuentos (origen = la propia línea)
            order._process_discount_lines(order_lines, existing_lines_map, wizard_lines_map)

            # 2. Regalos manuales
            order._process_standard_promotions(order_lines, existing_lines_map, wizard_lines_map)

            # 3. Regalos automáticos
            order._process_automatic_gifts(order_lines, existing_lines_map, wizard_lines_map)

            # --- Construcción de comandos para las líneas del Wizard ---
            wizard_line_cmds = []
            for data in wizard_lines_map.values():

                print("wizard_line_cmds  ========================")
                print(data)

                promo = self.env["promotion.benefit"].browse(data["promotion_id"])
                discount_line = data["discount"]

                print(promo.name)
                print(promo.promotion_type_id)
                print(promo.selection_type_id)

                if (promo.promotion_type_id.id == 2 or promo.promotion_type_id.id == 4) \
                        and (promo.selection_type_id.id == 2 or promo.selection_type_id.id == 1):
                    discount_line = 100

                wizard_line_cmds.append((0, 0, {
                    "promotion_id": data["promotion_id"],
                    "rule_id": data["rule_id"],
                    "discount": discount_line,
                    "qty_confirmation": data["qty_confirmation"],
                    "state": data["state"],
                    "rule_value": data["rule_value"],
                    "message": data["message"],
                    "name": data["name"],
                    "lines_ids": [(6, 0, list(data["lines_ids"]))],
                }))

            # 4. Crear Asistente (Wizard)
            wizard = self.env["sale.order.promotion.wizard"].create({
                "order_id": order.id,
                "line_ids": wizard_line_cmds,
                "base": True,
            })
            print("Wizard creado:", wizard.id)

            # 5. Regalos / escritura final
            all_gift_cmds = order._process_gift_and_discount_commands(
                order_lines, existing_lines_map, wizard
            )

            print("all_gift_cmds   ===============================")
            print(all_gift_cmds)

            if all_gift_cmds:
                wizard.write({
                    "all_gift_line_ids": all_gift_cmds
                })

            # 6. Confirmación de Promociones
            wizard.with_context(skip_missing_check=True).action_confirm_promotions()
            print("✅ Promociones aplicadas")

    # =========================================================================
    # HELPERS
    # =========================================================================
    def _build_existing_lines_map(self):
        """Genera el mapa (product_id, sequence) -> line de las líneas del ERP."""
        return {
            (l.product_id.id, l.sequence): l
            for l in self.order_line
        }

    def _collect_origin_ids(self, parsed, existing_lines_map, label="ORIGEN"):
        """
        Recorre origin_gift_line_ids_offline y devuelve TODOS los origin_id
        encontrados (no solo el último). Escala a N líneas sin problema.
        """
        origin_ids = set()
        rule_value = 0

        for item in parsed or []:
            if not isinstance(item, dict):
                continue

            seq = item.get("sequence")
            prod = item.get("product_id")
            found = existing_lines_map.get((prod, seq))

            if not found:
                print("PRODUCTO %s NO ENCONTRADO" % label, seq, prod)
                continue

            origin_ids.add(found.id)
            allowed = item.get("total_allowed_gifts") or 0
            if allowed:
                rule_value = allowed

        return origin_ids, rule_value

    def _resolve_promo_rule_ids(self, vals, parsed=None):
        """
        Obtiene (promo_ids, rule_ids) de la línea.
        Si vienen vacíos (caso típico NxN), usa origin_gift_line_ids_offline
        y/o promotionRules.
        """
        rule_ids = list(vals.get("rule_ids") or [])
        promo_ids = list(vals.get("promotion_ids") or [])

        if rule_ids and promo_ids:
            return promo_ids, rule_ids

        parsed = parsed if parsed is not None else self._safe_json_load(
            vals.get("origin_gift_line_ids_offline")
        )
        for item in parsed or []:
            if not isinstance(item, dict):
                continue
            rid = item.get("rule_id")
            pid = item.get("promo_id")
            if rid and rid not in rule_ids:
                rule_ids.append(rid)
            if pid and pid not in promo_ids:
                promo_ids.append(pid)

        if not rule_ids or not promo_ids:
            for rule in (vals.get("promotionRules") or []):
                if not isinstance(rule, dict):
                    continue
                rid = rule.get("id")
                pid = rule.get("promo_id")
                if rid and rid not in rule_ids:
                    rule_ids.append(rid)
                if pid and pid not in promo_ids:
                    promo_ids.append(pid)

        # Emparejar longitudes: si hay rules sin promo, repetir el primero
        if rule_ids and promo_ids and len(promo_ids) < len(rule_ids):
            promo_ids = promo_ids + [promo_ids[0]] * (len(rule_ids) - len(promo_ids))

        return promo_ids, rule_ids

    def _upsert_wizard_line(
            self,
            wizard_lines_map,
            promo_id,
            rule_id,
            origin_ids,
            discount,
            rule_value,
            is_gift,
            split_by_origin=True):
        """
        Crea/actualiza entradas del wizard.

        - Descuentos (split_by_origin=True): key=(promo, rule, origin)
          → cada línea padre conserva su propio descuento/promo.
        - Regalos (split_by_origin=False): key=(promo, rule)
          → UNA sola entrada con TODOS los padres en lines_ids.
          Evita repetir PROMO0311 en la web y triplicar qty de BAG/81664.
        """
        if not promo_id or not rule_id:
            return

        if isinstance(origin_ids, int):
            origin_ids = [origin_ids]
        origin_ids = [oid for oid in (origin_ids or []) if oid]
        if not origin_ids:
            return

        if split_by_origin:
            for origin_id in origin_ids:
                key = (promo_id, rule_id, origin_id)
                if key not in wizard_lines_map:
                    wizard_lines_map[key] = {
                        "promotion_id": promo_id,
                        "rule_id": rule_id,
                        "discount": discount,
                        "rule_value": rule_value,
                        "qty_confirmation": True,
                        "state": "processed",
                        "is_gift": is_gift,
                        "lines_ids": set(),
                        "message": "Promocion Aplicada: OK -",
                        "name": "------",
                    }
                wizard_lines_map[key]["lines_ids"].add(origin_id)
        else:
            key = (promo_id, rule_id)
            if key not in wizard_lines_map:
                wizard_lines_map[key] = {
                    "promotion_id": promo_id,
                    "rule_id": rule_id,
                    "discount": discount,
                    "rule_value": rule_value,
                    "qty_confirmation": True,
                    "state": "processed",
                    "is_gift": is_gift,
                    "lines_ids": set(),
                    "message": "Promocion Aplicada: OK -",
                    "name": "------",
                }
            wizard_lines_map[key]["lines_ids"].update(origin_ids)

    # =========================================================================
    # DESCUENTOS — origen = la propia línea (product_id, sequence)
    # =========================================================================
    def _process_discount_lines(self, order_lines, existing_lines_map, wizard_lines_map):
        """
        Cada línea con discount > 0 se asocia a SÍ MISMA.
        No usa origin_gift_line_ids_offline (evita tomar el último origen del JSON
        cuando hay 2+ productos con la misma promo).
        Funciona igual con 2, 5 o 100 líneas.
        """
        for line in order_lines:
            if not isinstance(line, list) or len(line) < 3:
                continue

            vals = line[2]
            if vals.get("is_gift"):
                continue

            discount = vals.get("discount") or 0
            if not discount > 0:
                continue

            print("INIT _process_discount_lines")
            print(vals)

            product_id = vals.get("product_id")
            sequence = vals.get("sequence")
            qty = vals.get("product_uom_qty", 0)

            found_line = existing_lines_map.get((product_id, sequence))
            if not found_line:
                print("LINEA DESCUENTO NO ENCONTRADA", sequence, product_id)
                continue

            origin_id = found_line.id

            rule_ids = vals.get("rule_ids") or []
            promo_ids = vals.get("promotion_ids") or []

            for idx, rule_id in enumerate(rule_ids):
                promo_id = (
                    promo_ids[idx] if idx < len(promo_ids)
                    else (promo_ids[0] if promo_ids else False)
                )
                if not promo_id or not rule_id:
                    print("_process_discount_lines --- promo/rule NO ENCONTRADOS")
                    continue

                self._upsert_wizard_line(
                    wizard_lines_map,
                    promo_id,
                    rule_id,
                    origin_id,
                    discount,
                    qty,
                    is_gift=False,
                    split_by_origin=True,
                )

    # =========================================================================
    # REGALOS MANUALES
    # =========================================================================
    def _process_standard_promotions(self, order_lines, existing_lines_map, wizard_lines_map):
        """Procesa regalos manuales (is_gift + is_manual). Agrega TODOS los orígenes."""
        print("=========================================")
        print("_process_standard_promotions START")
        auto_selection = self.env.ref('mnsa_promotion_benefit.selection_type_automatico')

        for line in order_lines:
            if not isinstance(line, list) or len(line) < 3:
                continue

            vals = line[2]
            is_gift = vals.get("is_gift")
            is_manual = vals.get("is_manual")

            print(is_gift)
            print(is_manual)

            if not is_gift or not is_manual:
                continue

            print("_process_standard_promotions LINE")
            print(vals)

            parsed = self._safe_json_load(vals.get("origin_gift_line_ids_offline"))
            origin_ids, qty = self._collect_origin_ids(
                parsed, existing_lines_map, label="REGALO MANUAL"
            )

            if not origin_ids:
                continue

            promo_ids, rule_ids = self._resolve_promo_rule_ids(vals, parsed)

            print("_process_standard_promotions N2")
            print("promo_ids", promo_ids)
            print("rule_ids", rule_ids)
            print("origin_ids", origin_ids)

            for idx, rule_id in enumerate(rule_ids):
                promo_id = (
                    promo_ids[idx] if idx < len(promo_ids)
                    else (promo_ids[0] if promo_ids else False)
                )
                if not promo_id or not rule_id:
                    print("_process_standard_promotions --- NO ENCONTRADOS ???")
                    continue

                print("======PROMO == RULE", promo_id, rule_id)
                promo = self.env["promotion.benefit"].browse(promo_id)

                if promo:
                    print("PROMO ID PRINCIPAL:", promo.id)
                    print("PROMO NAME:", promo.name)
                    print("PROMO DB selection_type:", promo.selection_type_id.id)
                    print("AUTO ID:", auto_selection.id)
                    print("PROMO:", promo_id, "AUTO:", promo.selection_type_id == auto_selection)
                else:
                    print("PROMO NO ENCONTRADA:", promo_id)

                self._upsert_wizard_line(
                    wizard_lines_map,
                    promo_id,
                    rule_id,
                    origin_ids,
                    discount=100,
                    rule_value=qty,
                    is_gift=True,
                    split_by_origin=False,
                )

            print("===============================")
            print("WIZARD MANUAL")
            print(wizard_lines_map)

    # =========================================================================
    # REGALOS AUTOMÁTICOS
    # =========================================================================
    def _process_automatic_gifts(self, order_lines, existing_lines_map, wizard_lines_map):
        """Procesa regalos automáticos (is_gift y no manual). Agrega TODOS los orígenes."""
        print("=========================================")
        print("_process_automatic_gifts START")
        auto_selection = self.env.ref('mnsa_promotion_benefit.selection_type_automatico')

        for line in order_lines:
            if not isinstance(line, list) or len(line) < 3:
                continue

            vals = line[2]
            is_gift = vals.get("is_gift")
            is_manual = vals.get("is_manual")

            print(is_gift)
            print(is_manual)

            if not is_gift or is_manual:
                continue

            parsed = self._safe_json_load(vals.get("origin_gift_line_ids_offline"))
            origin_ids, qty = self._collect_origin_ids(
                parsed, existing_lines_map, label="REGALO AUTOMATICO"
            )

            if not origin_ids:
                continue

            promo_ids, rule_ids = self._resolve_promo_rule_ids(vals, parsed)

            print("_process_automatic_gifts N2")
            print("promo_ids", promo_ids)
            print("rule_ids", rule_ids)
            print("origin_ids", origin_ids)

            for idx, rule_id in enumerate(rule_ids):
                promo_id = (
                    promo_ids[idx] if idx < len(promo_ids)
                    else (promo_ids[0] if promo_ids else False)
                )
                if not promo_id or not rule_id:
                    print("_process_automatic_gifts --- NO ENCONTRADOS ???")
                    continue

                print("======PROMO == RULE", promo_id, rule_id)
                promo = self.env["promotion.benefit"].browse(promo_id)

                if promo:
                    print("PROMO ID PRINCIPAL:", promo.id)
                    print("PROMO NAME:", promo.name)
                    print("PROMO DB selection_type:", promo.selection_type_id.id)
                    print("AUTO ID:", auto_selection.id)
                    print("PROMO:", promo_id, "AUTO:", promo.selection_type_id == auto_selection)
                else:
                    print("PROMO NO ENCONTRADA:", promo_id)

                self._upsert_wizard_line(
                    wizard_lines_map,
                    promo_id,
                    rule_id,
                    origin_ids,
                    discount=100,
                    rule_value=qty,
                    is_gift=True,
                    split_by_origin=False,
                )

            print("===============================")
            print("WIZARD AUTO")
            print(wizard_lines_map)

    # =========================================================================
    # COMANDOS FINALES DE REGALOS
    # =========================================================================
    def _process_gift_and_discount_commands(self, order_lines, existing_lines_map, wizard):
        """
        Un solo comando por producto regalo (qty una vez).
        lines_ids = TODOS los padres del JSON (tags Origin Gift distintos).
        Si se crea un cmd por cada origen con la misma qty → BAG 5+5(+5)=10/15.
        """

        print("=== EXISTING LINES MAP ===")
        for (product_id, sequence), line in existing_lines_map.items():
            print(f"Producto: {product_id}, Secuencia: {sequence}, Line ID: {line.id}")

        all_gift_cmds = []
        seen_gift = set()

        for line in order_lines:
            if not isinstance(line, list) or len(line) < 3:
                continue

            vals = line[2]
            if not vals.get("is_gift"):
                continue

            product_tmpl_id = vals.get("product_tmpl_id")
            qty = vals.get("product_uom_qty")
            price = vals.get("price_unit")
            discount = 100

            if price is None or price == 0:
                product_tmpl_obj = self.env["product.template"].browse(product_tmpl_id)
                if product_tmpl_obj and self.pricelist_id:
                    print("PRECIO EN 0 — buscando en lista de precios")
                    price_list_value = self.pricelist_id._get_product_price(
                        product_tmpl_obj, qty, self.partner_id
                    )
                    price = price_list_value if price_list_value is not None else 0
                print("PRECIO OBTENIDO:", price)

            parsed = self._safe_json_load(vals.get("origin_gift_line_ids_offline"))
            print("PARSED", parsed)

            origin_ids = []
            for item in parsed or []:
                if not isinstance(item, dict):
                    continue
                found = existing_lines_map.get((item.get("product_id"), item.get("sequence")))
                if not found:
                    continue
                if found.id not in origin_ids:
                    origin_ids.append(found.id)

            promo_ids, rule_ids = self._resolve_promo_rule_ids(vals, parsed)
            promo_id = promo_ids[0] if promo_ids else False
            rule_id = rule_ids[0] if rule_ids else False

            if not origin_ids or not rule_id or not promo_id:
                print("GIFT SIN ORIGEN/PROMO/RULE", product_tmpl_id)
                continue

            matched_line = wizard.line_ids.filtered(
                lambda l, r=rule_id, p=promo_id: l.rule_id.id == r and l.promotion_id.id == p
            )[:1]

            if not matched_line:
                print("NO MATCH EXACTO:", origin_ids, rule_id, promo_id)
                continue

            # Deduplicar: un cmd por (tmpl, promo, rule)
            key = (product_tmpl_id, promo_id, rule_id, matched_line.id)
            if key in seen_gift:
                print("DUPLICADO IGNORADO:", key)
                continue
            seen_gift.add(key)

            print("✔ Gift único:", product_tmpl_id, "qty=", qty, "origins=", origin_ids)

            all_gift_cmds.append((0, 0, {
                "product_id": product_tmpl_id,
                "qty": qty,
                "stock": 100,
                "price": price,
                "approve": True,
                "obtained": False,
                "discount": discount,
                "lines_ids": [(6, 0, origin_ids)],
                "promotion_line_id": matched_line.id,
            }))

        return all_gift_cmds

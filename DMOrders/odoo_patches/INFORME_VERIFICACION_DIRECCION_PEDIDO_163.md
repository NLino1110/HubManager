# Informe: verificación de dirección enviada desde app móvil

**Fecha del payload:** 2026-07-29 16:35:36  
**Pedido local:** id `163` / referencia `M163-AD29072026`  
**Cliente:** AGUILAR OBANDO DIEGO PATRICIO (`_partner_id`: **30361**)

---

## 1. Conclusión

Se verifica que la dirección enviada desde el aplicativo móvil **corresponde a la seleccionada por el usuario** en el combo de direcciones del pedido.

Evidencia en el payload externo:

| Campo | Valor | Significado |
|--------|--------|-------------|
| `_partner_invoice_id` | **84080** | Id de la dirección (contacto hijo) usada como factura |
| `_partner_shipping_id` | **84080** | Misma dirección usada como entrega |
| `partner_display_address` | `CALLE SUCRE Y ANA PAREDES FRENTE A FARMACIA SANA SANA` | Texto de la dirección seleccionada |

Ambos ids son **iguales (84080)**, coherente con la lógica actual de la app: al guardar/enviar se toma el `id` de `ddfAddress.SelectedItem` y se asigna a factura y envío.

La dirección **no** es el cliente padre (`30361`); es un registro hijo en `res_partner` (tipo dirección / `other`) con id **84080**.

---

## 2. Limitación actual de la app (direcciones inactivas)

En la versión actual del aplicativo móvil:

- El listado de direcciones carga todos los contactos hijos con `_parent_id = cliente` y `_type = 'other'`.
- **No se filtra por estado activo/inactivo** (`active`).
- Por tanto, **se despliegan todas las direcciones existentes** del cliente (incluidas inactivas, si las hubiera en SQLite).

**Implicación:** el usuario puede seleccionar (y enviar) una dirección inactiva si aparece en el combo. Cualquier control de “solo direcciones activas” debería implementarse en un cambio futuro (filtro en carga del dropdown y/o validación al guardar).

---

## 3. Resumen del pedido (payload)

```json
{
  "data": {
    "promo_data": "",
    "sale_order": {
      "id": 163,
      "id_referencia": "M163-AD29072026",
      "state": "draft",
      "state_view": "ACTIVO",
      "date_order": "2026-07-29 10:17:06",
      "mobile_create_date": "2026-07-29 10:17:06",
      "write_date": "2026-07-29 10:32:26",
      "timestamp_envio": "2026-07-29 16:35:36",
      "_partner_id": 30361,
      "partner_display_name": "AGUILAR OBANDO DIEGO PATRICIO",
      "partner_display_status": "Activo",
      "partner_display_address": "CALLE SUCRE Y ANA PAREDES FRENTE A FARMACIA SANA SANA",
      "_partner_invoice_id": 84080,
      "_partner_shipping_id": 84080,
      "_company_id": 1,
      "_center_id": 49,
      "_warehouse_id": 136,
      "_pricelist_id": 38,
      "sale_channel": 8,
      "partner_sale_id": 75473,
      "external_create_uid": 907,
      "external_guid": "63881e44360b4c7f89b0ea04e38b7fa3",
      "mobile_sync": true,
      "is_synchronized": false,
      "amount_untaxed": 2009.3718324,
      "amount_tax": 301.4059,
      "amount_total": 2310.7777324,
      "promotion_ids": [19, 22, 20, 1429],
      "order_line_count": 23
    }
  },
  "version": 1,
  "timestamp": "2026-07-29 16:35:36"
}
```

### Líneas (resumen)

- **23 líneas** en `order_line` (comandos Odoo `[0, 0, {...}]`).
- Productos de venta + bonificaciones (`is_gift: true`), varias con `is_manual: true`.
- Promos referenciadas: 19, 20, 22, 1429.

El JSON completo del payload (con todas las líneas y `promotionRules`) se conserva en el origen del envío / log de sync; este informe prioriza la verificación de dirección.

---

## 4. Cómo validar en Odoo / SQLite

```sql
-- Dirección enviada
SELECT id, name, street, street2, parent_id, type, active
FROM res_partner
WHERE id = 84080;

-- Otras direcciones del mismo cliente (activas e inactivas)
SELECT id, name, street, type, active
FROM res_partner
WHERE parent_id = 30361 AND type = 'other'
ORDER BY id;
```

Si en Odoo `84080` es la dirección que el usuario eligió en tablet, queda confirmado el comportamiento esperado del envío.

---

## 5. Recomendación (opcional, futuro)

Filtrar direcciones en el dropdown:

```csharp
x._parent_id == CurrentPartner.id && x._type == "other" && x.active
```

(y/o validar al guardar que la dirección seleccionada siga activa).

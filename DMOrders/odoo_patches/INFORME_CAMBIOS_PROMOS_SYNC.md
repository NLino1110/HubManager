# Informe de cambios — DMOrders y SaleOrderExtend

**Proyecto:** HubManager / DMOrders (.NET MAUI 10) + módulo Odoo `sale.order`  
**Fecha del informe:** 28 de julio de 2026  
**Alcance:** sincronización de promociones/regalos con Odoo, estabilidad de `sequence`, UI de envío de datos

---

## 1. Resumen ejecutivo

Se corrigieron fallos donde, al sincronizar pedidos offline hacia Odoo:

1. Las **promociones/regalos se perdían** o se asociaban al producto incorrecto porque el `sequence` del padre cambiaba y `origin_gift_line_ids_offline` quedaba desfasado.
2. En **descuentos multi-producto**, solo una línea recibía bien la promo (se tomaba el “último” origen del JSON).
3. En **regalos con varios padres**, se **multiplicaban cantidades** y se **duplicaban tags** de promoción en la web.
4. En promos **NxN**, el regalo salía sin `promotion_ids`/`rule_ids` y Odoo no aplicaba nada.
5. Al usar **Enviar datos** (menú inferior) no se veían los números de pedido hasta cerrar sesión.

---

## 2. Error que presentaba antes

| # | Síntoma | Causa raíz |
|---|---------|------------|
| A | Al Confirmar/Aplicar promociones, se borraban regalos y se renumeraban líneas; al sync, Odoo no encontraba el padre | `SaveOrder` hacía `sequence = 1,2,3…` siempre; el JSON del gift seguía con el sequence viejo. Odoo matchea `(product_id, sequence)`. |
| B | Producto con descuento (ej. 55256): se veía `%` pero no `$DESC` ni promo; el otro producto (55314) sí | `_process_discount_lines` leía `origin_gift_line_ids_offline` y se quedaba con el **último** origen del array. |
| C | BAG / regalos con qty ×3 (15 en vez de 5) y `PROMO0311` repetida en padres | Una entrada de wizard/gift **por cada padre** con la misma cantidad. |
| D | Promo NxN (ROLDA 12+1): ni regalo ni promo en el padre en Odoo | Gift NxN solo llamaba `SetPromotionDataGift`; `promotion_ids`/`rule_ids` quedaban `[]`. |
| E | “Enviar datos” no mostraba `PED-…` hasta logout | No se recargaba la lista de pedidos tras sync. |

**Campo clave:** `origin_gift_line_ids_offline` (JSON en la línea regalo) indica de qué línea(s) padre nació la promo. Odoo (`SaleOrderExtend`) usa `(product_id, sequence)` para reaplicar promociones vía wizard.

---

## 3. Módulos / archivos afectados

### 3.1 App móvil DMOrders (.NET)

| # | Módulo / área | Ruta completa |
|---|---------------|---------------|
| 1 | Persistencia de pedido (sequence estable) | `DMOrders/Pages/Fragments/Orders/Crud.xaml.save.cs` |
| 2 | Alta de líneas (sequence sin contar gifts) | `DMOrders/Pages/Fragments/Orders/Crud.xaml.core.cs` |
| 3 | Escritura de orígenes en regalos | `DMSA.Models.Odoo/Promotions/Tools.v2.cs` |
| 4 | Alta automática NxN de regalos | `DMOrders/Pages/Fragments/Orders/modals/PromocionesViewer.xaml.items.cs` |
| 5 | Envío masivo de pedidos | `DMSA.Sync.Core/Update/Pusher/SaleOrders.cs` |
| 6 | Menú “Enviar datos” + refresh UI | `DMOrders/MainPageTab.xaml.cs` |

### 3.2 Odoo (parche entregado en el repo)

| # | Módulo | Ruta en el repo (copiar al addons Odoo) |
|---|--------|----------------------------------------|
| 7 | Extensión `sale.order` sync móvil | `DMOrders/odoo_patches/sale_order_extend.py` |
| 8 | Payload de referencia (documentación) | `DMOrders/odoo_patches/payload_correcto_promos_referencia.json` |

> **Nota:** `sale_order_extend.py` **no vive** dentro del runtime .NET; es el archivo a desplegar en el módulo Odoo que hereda `sale.order`.

---

## 4. Cambios por archivo (antes → después)

### 4.1 `Crud.xaml.save.cs`

**Ruta:** `DMOrders/Pages/Fragments/Orders/Crud.xaml.save.cs`

| | Descripción |
|--|-------------|
| **Antes** | Al guardar: `orderLine.sequence = ordinal` (1..n) para **todas** las líneas (productos + regalos). Tras borrar/reaplicar gifts, el padre cambiaba de N° (ej. 4 → 2) y el JSON seguía en 4. |
| **Después** | `AssignStableSequences`: conserva `sequence` de productos (`!is_gift` con seq > 0); asigna libres a gifts/nuevos; `RemapOriginGiftOfflineSequences` + `AlignGiftOriginsToCurrentParents` si hay desfase. |
| **Mejora** | El padre mantiene identidad estable para el match Odoo `(product_id, sequence)`. |

---

### 4.2 `Crud.xaml.core.cs`

**Ruta:** `DMOrders/Pages/Fragments/Orders/Crud.xaml.core.cs`

| | Descripción |
|--|-------------|
| **Antes** | `sequence = OrderLines.Count + 1` (incluía regalos → desplazaba sequences de productos). Merge solo sumaba qty sin invalidar gifts viejos. |
| **Después** | `GetNextProductSequence()` (solo no-gift). En merge: `InvalidatePromotionsForMergedProductLine` quita regalos auto ligados a ese `product_id` para recalcular limpio. |
| **Mejora** | Sequences de producto coherentes desde el alta; evita JSON huérfano tras sumar cantidades. |

---

### 4.3 `Tools.v2.cs` — `SetPromotionDataGift`

**Ruta:** `DMSA.Models.Odoo/Promotions/Tools.v2.cs`

| | Descripción |
|--|-------------|
| **Antes** | Solo serializaba el **primer** ítem de `ProductSequenceApplyList` (`FirstOrDefault`). |
| **Después** | Serializa **todos** los orígenes (deduplicados por product/sequence/promo/rule). |
| **Mejora** | Regalos multi-padre llevan la lista completa en `origin_gift_line_ids_offline`. |

---

### 4.4 `PromocionesViewer.xaml.items.cs` (NxN)

**Ruta:** `DMOrders/Pages/Fragments/Orders/modals/PromocionesViewer.xaml.items.cs`

| | Descripción |
|--|-------------|
| **Antes** | En alta NxN (`!productExistsInOrder`) solo `SetPromotionDataGift` → `promotion_ids`/`rule_ids` vacíos en el gift. |
| **Después** | También `SetPromotionData(line, …)` antes del gift. |
| **Mejora** | El payload del regalo NxN lleva `promotion_ids` y `rule_ids`; Odoo puede armar el wizard. |

**Ejemplo payload corregido (regalo):**
```json
"promotion_ids": [1604],
"rule_ids": [5286],
"origin_gift_line_ids_offline": "[{\"sequence\":1,\"product_id\":3610,...,\"promo_id\":1604,\"rule_id\":5286}]"
```

---

### 4.5 `SaleOrders.cs` — `SendAllSaleOrders`

**Ruta:** `DMSA.Sync.Core/Update/Pusher/SaleOrders.cs`

| | Descripción |
|--|-------------|
| **Antes** | `Task` void-like: enviaba pedidos y no devolvía resultado usable para UI. |
| **Después** | `Task<List<string>>`: retorna etiquetas (`erp_name` o `id_referencia`) de pedidos OK. |
| **Mejora** | Permite mostrar resumen de números sincronizados en el menú. |

---

### 4.6 `MainPageTab.xaml.cs` — Enviar datos

**Ruta:** `DMOrders/MainPageTab.xaml.cs`

| | Descripción |
|--|-------------|
| **Antes** | Sync + hide loading; **sin** recargar lista; sin alert de números; textos con encoding dañada. |
| **Después** | Tras sync: `tabOrders.ReloadData()` + `SelectTab("orders")` + alert con lista de pedidos; textos en español vía escapes Unicode (`\u00f3`, etc.). |
| **Mejora** | Ya no hace falta cerrar sesión para ver el `PED-…` / `erp_name`. |

---

### 4.7 `sale_order_extend.py` (Odoo)

**Ruta en repo:** `DMOrders/odoo_patches/sale_order_extend.py`  
**Destino Odoo:** módulo que hace `_inherit = "sale.order"` (reemplazar el archivo/clase original).

| Método / área | Antes | Después |
|---------------|-------|---------|
| `_process_discount_lines` | Origen = último ítem del JSON offline | Origen = **la propia línea** `(product_id, sequence)` |
| Regalos auto/manual | Un wizard entry por padre → duplicaba promo y qty | `split_by_origin=False`: **una** entry `(promo, rule)` con **todos** los padres en `lines_ids` |
| Gift commands | Un cmd por cada origen × misma qty | **Un** cmd por producto regalo; qty una vez; `lines_ids` = todos los padres |
| Promo/rule vacíos (NxN) | Fallaba el loop | `_resolve_promo_rule_ids`: fallback a offline JSON / `promotionRules` |

**Mejoras Odoo:**
- Varias promos en el mismo padre se asocian correctamente (descuento + bonificación).
- Varias líneas con la misma promo de descuento no se pisan.
- Cantidades de regalo no se multiplican por número de padres.
- Origin Gift Line muestra N padres distintos (tags), no N veces la misma qty.

---

## 5. Mejoras implementadas (checklist)

- [x] Sequence estable de líneas producto al guardar / reaplicar promos  
- [x] Sequence de alta sin contaminar con regalos (`GetNextProductSequence`)  
- [x] Invalidación de gifts al merge de cantidad  
- [x] JSON de origen con todos los padres cuando aplica  
- [x] Descuentos Odoo asociados línea a línea  
- [x] Regalos multi-padre sin ×N en cantidad ni promo duplicada  
- [x] NxN con `promotion_ids`/`rule_ids` + fallback Odoo  
- [x] Refresh de lista + resumen tras “Enviar datos”  
- [x] Textos UI restaurados (encoding)  

---

## 6. Flujo corregido (vista rápida)

```text
App: alta líneas → sequence estable (solo productos)
App: aplicar promos → gifts con origin JSON + promotion_ids/rule_ids
App: guardar → AssignStableSequences (no renumerar padres)
App: sync → Create pedido sin gifts + external_payload
Odoo: create → _procesos_especiales_externa
     → descuentos por línea propia
     → regalos: 1 wizard/(promo,rule) + 1 gift-cmd/producto
     → action_confirm_promotions
UI: Enviar datos → reload pedidos + alert con erp_name
```

---

## 7. Cómo probar (sugerido)

1. Pedido con producto + promos (descuento multi-SKU + regalos + NxN).  
2. Guardar / Confirmar / Aplicar → modificar → volver a aplicar.  
3. Verificar en SQLite: padre conserva `sequence`; gift JSON apunta a ese sequence; gift tiene `promotion_ids`/`rule_ids`.  
4. Sincronizar con **Enviar datos**: alert con números + lista con `erp_name` sin logout.  
5. En Odoo web: padres con sus promos; BAG/regalos con qty correcta; Origin Gift con padres distintos.

---

## 8. Archivos de soporte / referencia

| Archivo | Uso |
|---------|-----|
| `DMOrders/odoo_patches/sale_order_extend.py` | Código Odoo a desplegar |
| `DMOrders/odoo_patches/payload_correcto_promos_referencia.json` | Ejemplo payload correcto vs síntomas incorrectos (PYUNKANG) |

---

## 9. Fuera de alcance / no modificado

- Módulos en carpetas `back/`, `bk/`, `modals/bk/` (código legado / backup).  
- Lógica de wizard Odoo `sale.order.promotion.wizard` en sí (solo el orquestador `SaleOrderExtend`).  
- Cambio de diseño de Odoo a otra clave distinta de `(product_id, sequence)` (sigue siendo esa).

---

*Informe generado a partir de los cambios realizados en la sesión de trabajo DMOrders / promociones offline-sync.*

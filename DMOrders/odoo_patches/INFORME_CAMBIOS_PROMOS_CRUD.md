# Informe: cambios promociones / CRUD pedido

Fecha de consolidación: 2026-08-04  
Alcance: DMOrders (HubManager) — guardado de pedido, modal de promociones, borrado de producto padre.

---

## 1. Resumen ejecutivo

| Problema | Causa raíz | Solución |
|----------|------------|----------|
| App se cierra al pulsar **No eliminar** | Doble `PopModalAsync` + excepciones no controladas en WinUI | Un solo PopModal; try/catch; cierre seguro |
| `Object reference not set…` al guardar | NRE en `PrepareDiscount` / `promotionRules` / loading / nulls | Null-safe + no reprocesar descuentos ya aplicados |
| **No eliminar** no debía saltarse el modal | Intento intermedio saltaba `ApplyPromo` | Conserva promos **y** abre modal |
| Borrar producto padre dejaba promos/regalos | Delete usaba lógica incompleta | Reusa limpieza por origen/ids y quita entradas de promo |

---

## 2. Antes / después por flujo

### 2.1 Guardar con promociones ya aplicadas

**Antes**

- Diálogo Sí/No poco claro.
- “No eliminar” seguía a guardar + aplicar; a veces crash.
- `ApplyPromo` hacía `PopModal` y el `finally` de guardar otro más.
- Loading abierto al mostrar popup de promos.

**Error**

- Cierre de app (`UnhandledException` → `Debugger.Break` en WinUI).
- `NullReferenceException` al reaplicar descuentos.

**Después**

- Texto: Sí = limpiar y recalcular; No = conservar y abrir modal.
- Un PopModal (solo en `ButtonSave` finally).
- Hide loading antes del modal.
- Excepciones mostradas en alerta, sin tumbar la app.

### 2.2 PrepareDiscount / promotionRules

**Antes**

- Acceso directo a `lineToDiscount.promotionRules` y `.Exists` / `.Add`.
- Reprocesaba líneas con descuento ya aplicado (“No eliminar”).

**Error**

- `Object reference not set to an instance of an object` en esa zona.

**Después**

- Guards; lista nunca null; skip si `discount > 0`.
- Fallo de PrepareDiscount no impide abrir el modal.

### 2.3 Eliminar producto padre

**Antes**

- Solo intentaba quitar gifts vía `promotionDataList` / `related_product_tmpl_ids`.
- Promos y regalos podían quedar huérfanos.

**Después**

- Confirma: se eliminarán promociones y regalos.
- `ClearPromotionsOnParentQtyChange(..., removePromoEntries: true)`.

---

## 3. Archivos / pantallas modificados

Rutas desde `HubManager\`.

| Archivo | Pantalla / rol | Motivo | Qué se documentó/cambió en código |
|---------|----------------|--------|-----------------------------------|
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.cs` | CRUD pedido — Guardar | Crash / NRE / flujo No eliminar | `ButtonSave_Clicked`, `EvalPromotions` |
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.promotion.cs` | Popup promociones | Doble PopModal / NRE descuentos | `PrepareDiscount`, `ApplyPromo`, `ClosePromoPopupSafeAsync` |
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.save.cs` | Persistencia pedido | NRE dirección null | Validación `ddfAddress.SelectedItem` |
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.core.cs` | Líneas / eliminar | Promos no se iban al borrar padre | `ClearPromotionsOnParentQtyChange`, `RemoveOrderLine*`, `ExistsLinkedGifts` |
| `DMOrders\Controls\Tools\UITools.cs` | Loading overlay | NRE SetNotify | Null-check `simplePopup` |
| `DMOrders\Services\Promotions\PromotionEngineRunner.v2.cs` | Motor promos | NRE lista null | `CanApplyPromotion` |
| `DMSA.Models.Odoo\Promotions\Tools.v2.cs` | Mapeo reglas | NRE Promotion/SaleOrder | `FromBenefitRule` |
| `DMSA.Models.Odoo\Native\sale_order_line.cs` | Modelo línea | Lista null / ítems null | Getter `promotionRules` |

La UI XAML del CRUD / PromocionesViewer no cambió de layout; los cambios son de lógica (.cs). Comentarios XML/`///` en cada método clave describen **ANTES / ERROR / DESPUÉS**.

---

## 4. Cómo probar

1. Pedido con promos → Guardar → **No eliminar** → debe abrir modal; no cerrar app.
2. Pedido con promos → Guardar → **Sí, eliminar** → limpia y recalcula con modal.
3. Agregar producto nuevo con promo → **No eliminar** → modal con nuevas/existentes.
4. Eliminar producto padre con regalos → confirmar → se van regalos y promos atadas.
5. Guardar sin dirección de facturación → toast, sin NRE.

---

## 5. Nota

La sincronización de stock se consultó en la misma sesión; **no hubo cambio de código** de stock (filtro por centro + bodegas default + `write_date`).

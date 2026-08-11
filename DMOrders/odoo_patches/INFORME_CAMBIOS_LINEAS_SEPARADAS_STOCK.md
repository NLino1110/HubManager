# Informe: líneas separadas del mismo producto + validación de stock acumulado

Fecha: 2026-08-05  
Alcance: DMOrders — CRUD de pedido (agregar líneas y cambiar cantidades en detalle).  
Estado: **PRUEBA** (flag reversible). No refuerza aún promos / borrado multi-línea del mismo SKU.

---

## 1. Resumen ejecutivo

| Objetivo | Antes | Después (prueba) |
|----------|--------|------------------|
| Mismo producto varias veces | **Merge**: una sola línea, suma qty | **Separar**: cada alta crea otra línea (sequence nuevo) |
| Validar stock | Solo la qty de esa acción / línea | **Suma** de líneas padre del mismo SKU vs stock |
| Regalos / promos en la suma | N/A | **No** entran (`!is_gift`) |

---

## 2. Antes / después por flujo

### 2.1 Agregar el mismo producto (selector / botón añadir)

**Antes**

- `OnAddLine` / `OnAddLineNoRestrict` buscaban `FirstOrDefault(product_id && !is_gift)`.
- Si existía → sumaban `product_uom_qty` / `product_uom_qty_real` en esa línea.
- Stock: `cantidad_disponible < qty_sol` (solo la cantidad de la petición).

**Problema con stock + varias altas (si hubiera líneas separadas)**

- Stock 10, línea A = 6, alta de 5 → 5 ≤ 10 pasaba, total 11.

**Después**

- Flag `AllowSeparateSameProductLines = true` → no merge; siempre `OrderLines.Add` con `GetNextProductSequence()`.
- Stock: `ExceedsParentStock(productId, stock, qty_sol)` =
  - suma padres del mismo `product_id` (`!is_gift`) + qty a agregar > stock → alerta y no agrega.

**Revertir solo la separación**

```csharp
private const bool AllowSeparateSameProductLines = false;
```

(Con `false` vuelve el merge; la validación acumulada sigue siendo útil si en algún momento hay más de una línea.)

### 2.2 Cambiar cantidades en pestaña detalle

**Antes** (`ApplyValueChanges`)

- `ProductEditing.cantidad_disponible < product_uom_qty` solo sobre la línea en edición.

**Después**

- Padre (`!is_gift`): `ExceedsParentStock(..., excludeLine: CurrentSaleOrderLine)`  
  → suma **otras** líneas padre del mismo SKU + qty nueva.
- Regalo (`is_gift`): se mantiene validación de **una sola línea** vs stock.

### 2.3 Qué no cambia (esta prueba)

- Motor de promociones / modal de regalos (sin refuerzo multi-línea mismo SKU).
- Borrado de padre (sigue por `(product_id, sequence)` de esa línea).
- Listas de precios, impuestos, sync Odoo.

---

## 3. Archivos modificados

Rutas desde `HubManager\`.

| Archivo | Pantalla / rol | Cambio |
|---------|----------------|--------|
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.core.cs` | Alta de líneas | Flag separación; `SumParentProductQty` / `ExceedsParentStock`; stock en `OnAddLine` y `OnAddLineNoRestrict`; comentarios ANTES/DESPUÉS |
| `DMOrders\Pages\Fragments\Orders\Crud.xaml.cs` | Detalle qty | Stock acumulado en `ApplyValueChanges`; comentarios ANTES/DESPUÉS |

---

## 4. Helpers (comportamiento)

```
SumParentProductQty(productId, excludeLine?)
  → Σ product_uom_qty donde !is_gift && product_id == X && línea ≠ exclude

ExceedsParentStock(productId, stock, qtyToApply, excludeLine?)
  → SumParentProductQty + qtyToApply > stock
```

Ejemplo: stock 10.

| Acción | Resultado |
|--------|-----------|
| Primera línea qty 6 | OK (0+6 ≤ 10) |
| Segunda línea mismo SKU qty 5 | FALLO (6+5 > 10) |
| Segunda línea qty 4 | OK (6+4 ≤ 10) |
| Editar línea A de 6 → 7 con hermana en 4 | FALLO (4+7 > 10) |
| Línea regalo | No suma con padres; check individual |

---

## 5. Criterio de aceptación (pruebas)

- [ ] Mismo SKU dos veces → **dos líneas** distintas en el pedido (con flag en `true`).
- [ ] Una sola línea: stock se valida como siempre (esa qty vs disponible).
- [ ] Dos líneas padre mismo SKU: la suma no puede superar `cantidad_disponible`.
- [ ] Cambiar qty en detalle de un padre respeta la suma con las hermanas.
- [ ] Regalos / líneas promo no cuentan en la suma de stock de padres.
- [ ] Con `AllowSeparateSameProductLines = false` → vuelve el merge al agregar.

---

## 6. Riesgos conocidos (fuera de este alcance)

Con líneas separadas del mismo SKU, conviene probar aparte:

- Aplicar / limpiar promociones que asumen un solo padre por producto.
- Borrar un padre cuando hay otra línea del mismo `product_id` con promos compartidas.
- Remapeo de `origin_gift_line_ids_offline` si hay varios padres del mismo producto.

No forman parte de este cambio; solo separación + validación de stock.

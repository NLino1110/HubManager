# Informe: descuentos al modificar pedido (Aplicar de abajo)

Fecha: 2026-09-17  
Alcance: DMOrders — modal de promociones al actualizar un pedido con **No eliminar**.  
Tema: líneas tipo descuento (botón verde APLICAR) que perdían la relación promo/regla.

---

## 1. Resumen ejecutivo

| Problema | Causa raíz | Solución |
|----------|------------|----------|
| Al modificar un pedido con descuentos, **No eliminar** + solo **Aplicar de abajo** (sin el verde de cada fila) dejaba IDs incorrectos | El Aplicar de abajo **no confirmaba** las promos tipo 6. Solo cerraba el modal y persistía regalos. Las bonificaciones/NxN podían **pisar** `promotion_ids` / `rule_ids` de la línea padre | Al Aplicar / Aplicar y continuar se confirman **todas** las tipo 6 (las del botón verde), **aunque el % sea 0**, para grabar de nuevo la relación promo/regla |

Ejemplo real del fallo:

| | Original (primer guardado) | Tras No eliminar + Aplicar abajo sin verdes |
|--|----------------------------|---------------------------------------------|
| Línea descuento | `[17633]` / `[6841]` (PROMO0605) | `[25]` / `[4205]` (IDs del bonificado) |
| Línea bonificado | `[25]` / `[4205]` | `[25]` / `[4205]` (sin cambio) |

---

## 2. Antes / después por flujo

### 2.1 Modificar pedido con descuentos ya aplicados

**Antes**

- Diálogo: **Sí, eliminar** / **No eliminar**.
- **No eliminar** conservaba promos y abría el modal (correcto).
- En cada descuento (tipo 6) había que pulsar el **APLICAR verde** para grabar % e IDs.
- El **Aplicar de abajo** (rosa) no recorría esas reglas. Cerraba el popup y guardaba regalos/manuales.
- `ApplyPendingDiscountsAsync` existía pero:
  - no se llamaba desde el Aplicar de abajo;
  - solo miraba `promoDiscounts` (la fila visible);
  - **saltaba** reglas con `% = 0` (`discount > 0`).

**Error**

- Si el usuario revisaba descuentos (o ponía 0 / 5) y **no** pulsaba el verde, y luego iba a un bonificado y daba **Aplicar de abajo**:
  - la línea de descuento quedaba con IDs de otra promo (`25` / `4205` en lugar de `17633` / `6841`);
  - el % digitado no se grababa;
  - al sincronizar podía fallar la línea de descuento (relación rota).

**Después**

- **Aplicar** y **Aplicar y continuar** llaman a `ApplyPendingDiscountsAsync` **antes** de cerrar.
- Se recorren **todas** las promos tipo 6 del modal (0605, 0668, 0672…), no solo la visible.
- Entra **aunque el % sea 0**: se reafirma `promotion_ids` / `rule_ids` / `promotion_data`.
- Si el usuario escribió un % válido, también actualiza importes.
- Si el % quedó en 0 pero la línea ya tenía descuento (**No eliminar**), **no se borra** el %: solo se reafirman los IDs.
- Bonificaciones y regalos no se tocan en este paso.
- Si un % supera el máximo, alerta y **no cierra** el modal.

### 2.2 Qué no cambió

- El botón verde de cada descuento sigue aplicando esa fila al momento.
- **Sí, eliminar** sigue limpiando y recalculando.
- NxN / regalos automáticos y manuales: misma lógica de siempre.
- No se unieron IDs descuento + bonificado en la misma línea (punto 2/3 de la sesión: no implementados).

---

## 3. Archivos modificados

Rutas desde `HubManager\`.

| Archivo | Pantalla / rol | Motivo | Qué cambió |
|---------|----------------|--------|------------|
| `DMOrders\Pages\Fragments\Orders\modals\PromocionesViewer.xaml.items.cs` | Modal promociones — descuentos | El Aplicar de abajo no confirmaba tipo 6; % 0 se omitía | `ApplyPendingDiscountsAsync` (todas las tipo 6, incluye % 0); `EnumerateManualDiscountRules`; `ApplyDiscountRule` graba relación siempre y no pisa % existente si el campo es 0 |
| `DMOrders\Pages\Fragments\Orders\modals\PromocionesViewer.xaml.events.cs` | Botones Aplicar / Aplicar y continuar | No se invocaba la confirmación de descuentos | `OnApplyButtonClicked` y `OnApplyAndContinueButtonClicked` llaman a `ApplyPendingDiscountsAsync` antes de cerrar |

La UI XAML del modal **no cambió** de layout. Los cambios son de lógica (.cs).

---

## 4. Cómo probar

1. Pedido nuevo con descuentos tipo 6 → guardar (IDs correctos, ej. `17633` / `6841`).
2. Reabrir / modificar → **No eliminar**.
3. Entrar a cada descuento, poner % o dejar 0, **sin** pulsar el verde.
4. Ir a un bonificado y pulsar el **Aplicar de abajo**.
5. Verificar que la línea de descuento **siga** con su promo/regla (`17633` / `6841`), no con la del regalo.
6. Repetir dejando % en 0: debe quedar la relación; el % que ya tenía la línea no debe irse a 0.
7. % mayor al máximo: alerta y el modal no cierra.
8. Pedido solo con regalos (sin tipo 6): Aplicar de abajo igual que antes.

---

## 5. Nota

En la misma sesión se evaluaron dos refuerzos **no implementados**:

1. No pisar IDs al armar regalos/NxN sobre un padre que ya tiene descuento (merge).
2. Aviso UX si hay descuentos sin confirmar.

El punto implementado cubre el caso reportado (No eliminar + descuentos + solo Aplicar de abajo).

*Informe generado — DMOrders / promociones descuento — 17 septiembre 2026*

# Informes técnicos — Cobranzas (AccountPaymentCrud)

**Módulo:** DMCobranzas → pantalla de cobro  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Fecha:** Agosto 2026  
**Pantalla:** `AccountPaymentCrud` (Nuevo / Edición de cobro)

---

# Informe 1 — Asignación de valores, alertas, distribución y formato decimal

## Resumen ejecutivo

Se corrigió y mejoró la lógica de aplicación de montos a facturas en la pantalla de cobro: botón verde (✓), botón limpiar (✗), redistribución con confirmación y formato decimal en campos y totales. El problema principal era que valores como `285.72` se interpretaban como `28572`, lo que rompía el cálculo de sobrante, la redistribución y los totales mostrados.

---

## Archivos modificados

| Archivo | Rol |
|---------|-----|
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml.cs` | Lógica de asignación, alertas y redistribución |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml` | Binding de comandos y formato TOT. APLICADO |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceLineAiRow.xaml` | Entry con conversor decimal, label CH. POSF. |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceLineAiRow.xaml.cs` | Botones ✓ y limpiar |
| `DMCobranzas\Converters\DecimalToStringConverter.cs` | Conversor decimal N2 |

---

## 1. Botón verde (✓) — aplicar sobrante

### Antes

- El botón ✓ invocaba el mismo flujo que un cambio manual de valor.
- No asignaba el monto del encabezado a la fila clickeada si su campo estaba en `0`.
- Recalculaba y **limpiaba** las filas siguientes.
- El valor terminaba en la factura de abajo, no en la seleccionada.

### Después

- Comando dedicado: `OnApplyAmountCommand` → `HandleAmountChangeAsync(..., applyHeaderMonto: true)`.
- Calcula el sobrante global:

  ```
  sobrante = Monto $ − Σ(amount_asigned de todas las filas)
  ```

- Aplica **solo** en la fila clickeada:

  ```
  nuevo = Min(valor_actual + sobrante, saldo_factura)
  ```

- **No redistribuye ni limpia** otras filas.

### Cambios en código

**`AccountPaymentCrud.xaml`**

```xml
<controlLite:MultipleCobrosInvoiceLineAiRow
    dataItem="{Binding .}"
    ValueChangedCommand="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.OnValueChangedCommand}"
    ApplyAmountCommand="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.OnApplyAmountCommand}" />
```

**`MultipleCobrosInvoiceLineAiRow.xaml.cs`**

```csharp
if (ApplyAmountCommand?.CanExecute(item) == true)
{
    ApplyAmountCommand.Execute(item);
    return;
}
```

**`AccountPaymentCrud.xaml.cs`** (aprox. líneas 1784–1804)

```csharp
decimal sobrante = totalPayment - totalAssigned;
changedItem.amount_asigned = Math.Min(
    changedItem.amount_asigned + sobrante,
    changedItem.amount_residual);
// return sin redistribuir otras filas
```

---

## 2. Botón limpiar (✗) y alerta de redistribución

### Antes

- El botón limpiar solo escribía `"0.00"` en el Entry.
- No ejecutaba `ValueChangedCommand` → **no aparecía la alerta** de redistribución.

### Después

- Establece `amount_asigned = 0`.
- Ejecuta `OnValueChangedCommand`.
- Si la fila tenía valor previo (`LastAppliedAmount > 0`), muestra alerta de confirmación.

| Opción | Comportamiento |
|--------|----------------|
| **Aceptar** | Redistribuye el valor liberado en cascada hacia las facturas siguientes |
| **No aceptar** | Deja el 0; el sobrante queda disponible para aplicar manualmente con ✓ |

### Textos de alerta

**Redistribución (al limpiar):**

- **Título:** `Distribución automática`
- **Mensaje:** *"Se realizará una distribución automática del valor liberado hacia la siguiente factura más antigua pendiente. ¿Desea continuar?"*

**Exceso de monto:**

- **Título:** `Atención`
- **Mensaje:** *"El valor excede el máximo permitido ({maxPermitido:N2})."*

**`MultipleCobrosInvoiceLineAiRow.xaml.cs`**

```csharp
// ANTES
private void ClearValue(object obj)
{
    entryPagoImporte.Text = "0.00";
}

// DESPUÉS
private void ClearValue(object obj)
{
    var item = dataItem ?? obj as MultipleCobrosInvoiceLineAi;
    if (item == null || item.pf_promised_amount > 0) return;

    item.amount_asigned = 0;
    ValueChangedCommand?.Execute(item);
}
```

---

## 3. Distribución de valores

### Métodos nuevos — `AccountPaymentCrud.xaml.cs`

| Método | Función |
|--------|---------|
| `SumAssignedBefore(lines, beforeIndex)` | Suma lo asignado en filas anteriores (excluye CH. POSF.) |
| `AssignToFollowingRows(..., amount, resetFollowingFirst)` | Reparte un monto en las filas siguientes |

### Dos modos de redistribución

**A) Limpiar + Aceptar** (`resetFollowingFirst: false`):

- Usa `previousAmount` (valor liberado).
- **Suma** sobre lo ya aplicado en filas posteriores.
- Ejemplo: se liberan $285,72 → la factura 3 pasa de $14,28 hasta su saldo ($95,69) → el excedente continúa a la factura 4, etc.

**B) Cambio manual de monto** (`resetFollowingFirst: true`):

- Pone en 0 las filas siguientes.
- Redistribuye el presupuesto restante desde esa fila.

### Antes (problema al limpiar + Aceptar)

```csharp
montoRestante = totalPayment - acumuladoAnterior - changedItem.amount_asigned;
// Solo contaba filas ANTERIORES → ignoraba asignaciones posteriores
```

### Después

```csharp
if (userClearedAssignment && acceptRedistribution)
    AssignToFollowingRows(..., previousAmount, resetFollowingFirst: false);
else
    AssignToFollowingRows(..., montoRestante, resetFollowingFirst: true);
```

---

## 4. Conversor decimal (`DecimalToStringConverter`)

### Antes

- Entry con binding directo: `Text="{Binding amount_asigned, Mode=TwoWay}"`.
- TOT. APLICADO sin decimales: `StringFormat='TOT. APLICADO: ${0}'`.
- En cultura es-ES, `"285.72"` se parseaba como **28572** (punto interpretado como separador de miles).
- La UI mostraba `28572` y `TOT. APLICADO: $28572` → cálculos incorrectos.

### Después — `DMCobranzas\Converters\DecimalToStringConverter.cs`

**Convert (modelo → pantalla):**

```csharp
return dec.ToString("N2", culture);  // ej. "285,72"
```

**ConvertBack (pantalla → modelo):**

```csharp
// 1) Intenta parse con cultura actual
// 2) Fallback: normaliza coma → punto + InvariantCulture
```

**Entry en fila:**

```xml
Text="{Binding amount_asigned, Mode=TwoWay, Converter={StaticResource DecimalToStringConverter}}"
```

**Total aplicado:**

```xml
<!-- ANTES -->  StringFormat='TOT. APLICADO: ${0}'
<!-- DESPUÉS --> StringFormat='TOT. APLICADO: ${0:N2}'
```

---

## 5. Por qué no se veían / no recibían valor todas las facturas

### Síntoma reportado

El contador mostraba **No. Docs. 32** (todas en lista), pero:

- Solo algunas tenían monto aplicado.
- Al borrar o usar ✓, otras perdían valor o no recibían el sobrante.
- Parecía que no se cargaban o no se aplicaban todas las facturas.

### Causa 1 — Bug del botón verde: limpiaba otras filas

**Archivo:** `AccountPaymentCrud.xaml.cs`

Al pulsar ✓, el código anterior:

1. Calculaba el monto disponible solo con filas **anteriores** a la clickeada.
2. Asignaba todo ese bloque a la fila seleccionada.
3. Ponía en **0** todas las filas siguientes si el restante era 0.

**Efecto:** Con $300 en 3 facturas, al pulsar ✓ en la factura 4 las filas 2 y 3 **perdían su valor**. Parecía que solo una factura tenía cobro.

**Corrección:** ✓ aplica solo sobrante a la fila clickeada sin tocar las demás.

### Causa 2 — Bug decimal: el sistema creía que no había sobrante

Con montos mal parseados (`28572` en lugar de `285,72`):

```
Sobrante = 300 − (28572 + 1428 + …) → negativo o 0
```

- ✓ no asignaba a facturas pendientes.
- La redistribución al borrar fallaba.
- Las facturas **estaban en lista** pero **no recibían valor**.

**Corrección:** Conversor N2 + TOT. APLICADO con `{0:N2}`.

### Causa 3 — Redistribución al borrar mal calculada

Al limpiar y aceptar, `acumuladoAnterior` no consideraba lo ya aplicado en filas **posteriores** (ej. $14,28 en factura 3). El monto liberado no se repartía correctamente.

**Corrección:** `AssignToFollowingRows(..., previousAmount, resetFollowingFirst: false)`.

### Causa 4 — Comportamiento esperado: [Facturas] no asigna a todas

Al pulsar **Facturas**, la distribución automática:

- Recorre facturas por fecha (más antigua primero).
- Asigna hasta agotar el **Monto $**.
- El resto **sí aparece en la lista** con **$0,00**.

| Lo que parece | Lo que ocurre |
|---------------|---------------|
| "No se cargaron todas" | Sí se cargaron; no se **aplicó** monto a todas |
| **No. Docs.** | Total de facturas en la lista |
| **TOT. APLICADO** | Suma de lo aplicado, no cantidad de facturas |

### Causa 5 — Filtro preexistente: sin línea `payment_term`

**Archivo:** `AccountPaymentCrud.xaml.cs` (líneas 925–926)

```csharp
if (!paymentTermMap.TryGetValue(accountMoveItem.id, out var itemPaymentTerm))
    continue;  // esta factura NO entra a la lista
```

Facturas con saldo en SQLite **no se muestran** si no tienen línea `payment_term` en `account_move_line`. `BuildPaymentTermMapAsync` intenta recuperarla desde SQLite y Odoo; si falla, la factura queda fuera.

> **Nota:** Este filtro no se modificó en estos cambios.

### Resumen: lista vs. asignación

| Concepto | Qué representa |
|----------|----------------|
| **No. Docs.** | Facturas en la lista (tras filtros) |
| **TOT. APLICADO** | Suma de montos aplicados |
| **Monto $** | Techo del cobro actual |
| Facturas con $0,00 | En lista, sin monto aplicado (normal si el monto ya se repartió) |

---

## Flujo de usuario de referencia (Informe 1)

```
Monto $ = 300  →  [Facturas]
  → Factura 1 (CH. POSF.) = 0,00      ← visible, bloqueada
  → Factura 2 = 285,72               ← auto-asignada
  → Factura 3 = 14,28                ← auto-asignada
  → Facturas 4–32 = 0,00             ← visibles, sin monto

[✗] Factura 2  →  Aceptar
  → 285,72 repartido en cascada a facturas 3, 4, 5…

[✗] Factura 2  →  No aceptar  →  [✓] Factura 4
  → Factura 4 recibe el sobrante ($285,72)
```

---

## Mejoras implementadas — Informe 1

1. Comando separado para botón verde (`OnApplyAmountCommand`).
2. Sobrante global correcto al aplicar con ✓.
3. Alerta al limpiar con opción Aceptar / No aceptar.
4. Redistribución en cascada al liberar montos.
5. Conversor decimal N2 en Entry y total.
6. Corrección del bug 285.72 → 28572.
7. Uso de `dataItem` en botones para evitar errores por recycling del `CollectionView`.
8. Documentación de por qué no todas las facturas reciben valor automáticamente.

---
---

# Informe 2 — Cheque posfechado (`pf_promised_amount`)

## Resumen ejecutivo

Se integró el campo **`pf_promised_amount`** (cheque posfechado prometido) desde Odoo/SQLite hasta la UI de cobro. Si la factura tiene valor en ese campo, se muestra **CH. POSF.**, se bloquea la aplicación de pagos y se excluye de la distribución automática al pulsar **Facturas**, hasta que el cheque se deposite en Odoo (`pf_promised_amount = 0`).

---

## Origen de la data (cadena completa)

```
Odoo (account.move.pf_promised_amount)
        ↓ sincronización API
HubAccountMove.cs  → fields_array incluye "pf_promised_amount"
        ↓
SQLite local → tabla account_move, columna pf_promised_amount
        ↓
account_move.cs  → propiedad pf_promised_amount
        ↓
AccountMoveDb.cs → EnsureColumnAsync("pf_promised_amount", "REAL")
        ↓
LoadPaymentLinesForNew() / LoadPaymentLines()
        ↓
MultipleCobrosInvoiceLineAi.pf_promised_amount
        ↓
UI fila → Label "CH. POSF.: $ X,XX"
```

---

## Archivos del ecosistema

| Archivo | Estado | Rol |
|---------|--------|-----|
| `ApiManagerOdoo\Accounting\HubAccountMove.cs` | Ya existía | Solicita `pf_promised_amount` a Odoo |
| `DMSA.Models.Odoo\Accounting\account_move.cs` | Ya existía | Modelo + columna SQLite |
| `DMSA.Sync.Core\Database\Sqlite\Payments\AccountMoveDb.cs` | Ya existía | Migra columna `pf_promised_amount REAL` |
| `DMSA.Models.Odoo\DebitCollection\MultipleCobrosInvoiceLineAi.cs` | **Modificado** | Propiedad en fila de cobro |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml.cs` | **Modificado** | Carga, skip y validación |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceLineAiRow.xaml` | **Modificado** | Label + bloqueo UI |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceLineAiRow.xaml.cs` | **Modificado** | Guard en limpiar |

---

## 1. Columna y modelo en Odoo / SQLite

### `account_move.cs`

```csharp
[JsonProperty("pf_promised_amount")]
[Column("pf_promised_amount")]
public decimal pf_promised_amount { get; set; }
```

### `HubAccountMove.cs`

```csharp
string[] fields_array = {
    // ...
    "pf_promised_amount",
    // ...
};
```

### `AccountMoveDb.cs`

```csharp
await EnsureColumnAsync("pf_promised_amount", "REAL");
```

---

## 2. Modelo de fila de cobro — `MultipleCobrosInvoiceLineAi.cs`

**Antes:** no existía `pf_promised_amount`.

**Después:**

```csharp
[JsonIgnore]
[Ignore]
public decimal pf_promised_amount { get; set; }

[JsonIgnore]
[Ignore]
public bool HasPostdatedCheckAmount => pf_promised_amount > 0;

[JsonIgnore]
[Ignore]
public bool CanApplyPayment => pf_promised_amount <= 0;
```

| Propiedad | Uso |
|-----------|-----|
| `pf_promised_amount` | Valor del cheque posfechado |
| `HasPostdatedCheckAmount` | Visibilidad del label CH. POSF. |
| `CanApplyPayment` | Habilitar/deshabilitar Entry y botón limpiar |

---

## 3. Carga de datos

### Nuevo cobro — `LoadPaymentLinesForNew()`

**Antes:**

```csharp
MultipleCobrosInvoiceLineAi cobrosInvoiceLineAiAux = new()
{
    // ... sin pf_promised_amount
    amount_residual = accountMoveItem.amount_residual,
    seller = SellerName
};
// Siempre entraba a distribución automática
```

**Después:**

```csharp
pf_promised_amount = accountMoveItem.pf_promised_amount,

if (accountMoveItem.pf_promised_amount > 0)
{
    cobrosInvoiceLineAiAux.amount_asigned = 0;
    multipleCobrosInvoiceLinesAiAux.Add(cobrosInvoiceLineAiAux);
    continue;  // NO entra a distribución automática [Facturas]
}
```

### Edición de cobro — `LoadPaymentLines()`

```csharp
var accountMoveItem = await accountMoveDb.GetItemAsync(x => x.id == line.invoice_id);
if (accountMoveItem != null)
    line.pf_promised_amount = accountMoveItem.pf_promised_amount;
```

En edición se refresca desde SQLite por si el valor cambió en Odoo tras depositar el cheque.

---

## 4. UI — campo CH. POSF.

### `MultipleCobrosInvoiceLineAiRow.xaml`

```xml
<Label Text="{Binding pf_promised_amount, StringFormat='CH. POSF.: $ {0:N2}'}"
       FontSize="11"
       FontAttributes="Bold"
       TextColor="DarkOrange"
       IsVisible="{Binding HasPostdatedCheckAmount}"/>

<Entry IsEnabled="{Binding CanApplyPayment}" ... />
<Button IsEnabled="{Binding CanApplyPayment}" ... />  <!-- limpiar -->
<!-- Botón verde sigue activo para mostrar alerta -->
```

---

## 5. Validaciones y bloqueos

### Regla de negocio

> Si `pf_promised_amount > 0` → la factura tiene cheque posfechado pendiente → **no se puede aplicar cobro** hasta que Odoo ponga el valor en 0 (cheque depositado).

### Matriz de comportamiento

| Acción | Con CH. POSF. > 0 |
|--------|-------------------|
| **[Facturas]** carga auto | Visible en lista; `amount_asigned = 0`; excluida de distribución |
| **Entry monto** | Deshabilitado |
| **Botón limpiar (✗)** | Deshabilitado |
| **Botón verde (✓)** | Muestra alerta; no aplica |
| **Redistribución automática** | Salta esa fila en `AssignToFollowingRows` |

### Validación en código — `AccountPaymentCrud.xaml.cs`

```csharp
private static bool HasPostdatedCheckAmount(MultipleCobrosInvoiceLineAi item) =>
    item.pf_promised_amount > 0;

if (HasPostdatedCheckAmount(changedItem))
{
    await DisplayAlertAsync(
        "Atención",
        "No es posible aplicar valor a este comprobante porque tiene cheque posfechado por cobrar asignado.",
        "Aceptar");
    return;
}

// En redistribución:
if (HasPostdatedCheckAmount(current))
    continue;
```

### Alerta cheque posfechado

- **Título:** `Atención`
- **Mensaje:** *"No es posible aplicar valor a este comprobante porque tiene cheque posfechado por cobrar asignado."*

---

## Mejoras implementadas — Informe 2

1. Campo **CH. POSF.** visible en cada fila cuando aplica.
2. Propiedad `pf_promised_amount` en modelo de fila de cobro.
3. Mapeo desde `account_move` en carga nueva y edición.
4. Exclusión de distribución automática al pulsar **Facturas**.
5. Bloqueo de Entry y botón limpiar (`CanApplyPayment`).
6. Alerta al pulsar ✓ en factura bloqueada.
7. Exclusión en redistribución manual/automática.
8. Reutilización de infraestructura Odoo/SQLite existente.

---

## Nota operativa

El desbloqueo depende de **Odoo**: cuando el cheque se deposita y `pf_promised_amount` pasa a `0`, tras sincronizar y recargar facturas la fila vuelve a ser aplicable con normalidad.

---

## Plan de pruebas sugerido

### Informe 1 — Asignación y decimales

| # | Paso | Resultado esperado |
|---|------|-------------------|
| 1 | Monto $300 → Facturas | Decimales correctos (285,72 / 14,28); TOT. APLICADO: $300,00 |
| 2 | ✓ en factura con sobrante | Solo esa factura recibe el pendiente |
| 3 | ✗ factura con valor → Aceptar | Valor liberado en cascada a siguientes |
| 4 | ✗ factura → No aceptar → ✓ otra | Sobrante aplicado a la elegida |
| 5 | ✓ con todo aplicado | No limpia otras filas |

### Informe 2 — Cheque posfechado

| # | Paso | Resultado esperado |
|---|------|-------------------|
| 1 | Factura con CH. POSF. > 0 | Label visible; Entry/✗ deshabilitados |
| 2 | Facturas con CH. POSF. | Esa fila queda en $0,00; no consume el monto |
| 3 | ✓ en factura CH. POSF. | Alerta de bloqueo |
| 4 | Tras depositar cheque en Odoo + sync | Factura vuelve a ser aplicable |

---

*Documento generado a partir de los cambios en la rama de trabajo de Cobranzas — AccountPaymentCrud.*

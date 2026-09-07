# Informe técnico — Anticipos de cliente (Cobranzas)

**Módulo:** DMCobranzas  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Fecha:** 25 de agosto de 2026  
**Proceso:** Anticipo total, anticipo parcial y persistencia de diferencia

---

## Resumen ejecutivo

Se habilitó el **anticipo parcial**: el cliente puede pagar un monto mayor al aplicado a facturas (ej. cobra $50, aplica $25, resto $25 como anticipo). Se eliminó el bloqueo por `EvalLinesRequired()` y se reforzaron validaciones de saldo antes de guardar.

---

## Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml` | UI monto, facturas, guardar |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml.cs` | Lógica anticipo y validación |
| `DMCobranzas\Services\Templates\Processor.cs` | Ticket recibo: línea ANTICIPO |
| `DMSA.Models.Odoo\DebitCollection\MultipleCobrosInvoiceLine.cs` | `MontoAplicadoTotal`, `Diferencia` |

---

## Problema original

Cobro **$50**, aplicación **$25** → alerta:

> *No se puede guardar hasta asignar todo el valor seleccionado.*

**Causa:** `EvalLinesRequired()` exigía repartir todo el monto entre facturas pendientes compatibles, impidiendo anticipo parcial.

---

## Escenarios al guardar (`btnSave_Clicked`)

| Condición | Comportamiento |
|-----------|----------------|
| `detailsCount == 0` | Diálogo anticipo **total** |
| Facturas cargadas, `totalAssigned == 0` | Diálogo anticipo **total** |
| `0 < totalAssigned < totalPayment` | Validar saldos → diálogo anticipo **parcial** |
| `totalAssigned == totalPayment` | Guarda normal (sin anticipo) |
| Aplicación > saldo documento | Alerta, no guarda |
| `totalAssigned > totalPayment` | Alerta, no guarda |

---

## Métodos clave

### `TryValidateAssignmentsBeforeSaveAsync()` (aprox. 1246–1278)

- `amount_asigned` vs. `amount_residual` por línea
- `totalAssigned` vs. `totalPayment`

### `btnSave_Clicked()` (aprox. 1281–1333)

- Eliminado `EvalLinesRequired()`
- Diálogo anticipo parcial con montos aplicado / total / diferencia

### `PerformSaveAsync(bool asAnticipo)` (aprox. 1333+)

Persistencia:

```csharp
decimal assignedTotal = multipleCobrosInvoiceLineAi.Sum(x => x.amount_asigned);

multipleCobrosInvoiceLine.MontoAplicadoTotal = assignedTotal;
multipleCobrosInvoiceLine.Diferencia = asAnticipo
    ? multipleCobrosInvoiceLine.Amount
    : Math.Max(0, (multipleCobrosInvoiceLine.Amount ?? 0) - assignedTotal);
```

| Modo | `lines` guardadas | `Diferencia` |
|------|-------------------|--------------|
| Anticipo total (`asAnticipo=true`) | vacías | monto completo |
| Anticipo parcial | solo líneas con `amount_asigned > 0` | monto − aplicado |
| Cobro normal (100% aplicado) | líneas con asignación | 0 |

Resumen automático anticipo total:

```csharp
if (asAnticipo && string.IsNullOrWhiteSpace(multipleCobrosInvoiceLine.Resumen))
    multipleCobrosInvoiceLine.Resumen = "ANTICIPO DE CLIENTE";
```

---

## Textos de diálogo / alerta

| Título | Mensaje |
|--------|---------|
| Anticipo de cliente | *No se han aplicado valores a sus facturas. ¿Desea generar el recibo como anticipo?* |
| Anticipo de cliente | *¿Desea generar el valor como anticipo ya que no cuenta con valores asignados a sus facturas?* |
| Anticipo de cliente | *Se aplicaron $X de $Y. ¿Desea guardar el saldo restante ($Z) como anticipo?* |
| Atención | *Las siguientes aplicaciones superan el saldo del documento:* + listado |
| Atención | *El total aplicado ($X) supera el monto del cobro ($Y).* |

---

## Campos SQLite

| Tabla | Columna | Uso |
|-------|---------|-----|
| `multiple_cobros_invoice_line` | `amount` | Monto forma de pago |
| `multiple_cobros_invoice_line` | `MontoAplicadoTotal` | Σ aplicado a documentos |
| `multiple_cobros_invoice_line` | `Diferencia` | Anticipo / saldo sin aplicar |
| `multiple_cobros_invoice_line_ai` | `amount_asigned` | Aplicado por documento |
| `multiple_cobros_invoice_line_ai` | `amount_residual` | Saldo documento |

---

## Impresión en ticket recibo

**Archivo:** `Processor.Template_MultipleCobrosInvoice()`

| Concepto | Cálculo |
|----------|---------|
| TOTAL F/P | Σ `line.Amount` |
| TOTAL CANC. | Σ `amount_asigned` |
| ANTICIPO | F/P − CANC. si > 0 |

Anticipo puro sin documentos: sección DOCUMENTOS muestra *ANTICIPO DE CLIENTE*.

*(Detalle completo de impresión en informe Impresión.)*

---

## Flujo ejemplo — anticipo parcial

```
1. Monto cobro: $50.00
2. Cargar facturas → aplicar $25.00 a FACT # …
3. GUARDAR
4. Diálogo: "Se aplicaron $25.00 de $50.00… ¿anticipo $25.00?"
5. Aceptar → Diferencia = 25, MontoAplicadoTotal = 25
```

---

## Plan de pruebas

| # | Caso | Esperado |
|---|------|----------|
| 1 | $50 cobro, $25 aplicado, guardar | Diálogo parcial; Diferencia $25 |
| 2 | $50 cobro, $0 aplicado | Diálogo anticipo total |
| 3 | Aplicar > SALDO | Alerta; no guarda |
| 4 | Aplicar $60 en cobro $50 | Alerta total aplicado |
| 5 | Ticket recibo mixto | TOTAL CANC. + ANTICIPO correctos |

---

*Informe Anticipos — Cobranzas Agosto 2026*

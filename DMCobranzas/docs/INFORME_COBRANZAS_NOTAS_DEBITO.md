# Informe técnico — Notas de débito (Cobranzas)

**Módulo:** DMCobranzas + DMSA.Sync.Core  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Fecha:** 25 de agosto de 2026  
**Proceso:** Sync, identificación y visualización de notas de débito en cobranzas

---

## Resumen ejecutivo

Las **notas de débito** de cliente se sincronizan junto con las facturas (`move_type = out_invoice`) y se distinguen con el campo booleano **`is_nota_debito`** desde Odoo. En la app se muestran en cobros, popups y tickets con el prefijo **`NDBI`**; las facturas usan **`FACT #`**.

---

## Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `ApiManagerOdoo\Accounting\HubAccountMove.cs` | Campo `is_nota_debito` en sync API |
| `DMSA.Models.Odoo\Accounting\account_move.cs` | Modelo + columna SQLite |
| `DMSA.Sync.Core\Database\Sqlite\Payments\AccountMoveDb.cs` | Migración columna; queries con saldo |
| `DMSA.Sync.Core\Update\ServerPuller.Accounting.cs` | Acciones sync renombradas |
| `DMCobranzas\AppPages\UpdateData.xaml` / `.xaml.cs` | Textos actualización |
| `DMSA.Models.Odoo\Accounting\AccountMoveDocumentDisplay.cs` | Prefijos FACT # / NDBI |
| `DMSA.Models.Odoo\DebitCollection\MultipleCobrosInvoiceLineAi.cs` | Campo en línea de aplicación |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml.cs` | Copia `is_nota_debito` al cargar docs |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceLineAiRow.xaml` | UI lista cobro |
| `DMSA.Sync.Core\Controls\Popups\PopupSelectInvoice.cs` | Últimas 20 / Detalle general |
| `DMCobranzas\Services\Templates\Processor.cs` | Ticket recibo DOCUMENTOS |

---

## Modelo en Odoo

| Campo | Valor factura | Valor nota de débito |
|-------|---------------|----------------------|
| `move_type` | `out_invoice` | `out_invoice` |
| `is_nota_debito` | `false` | `true` |
| `docnum_mask` | ej. `026-013-000001792` | ej. `026-013-000000123` |
| `amount_residual` | saldo pendiente | saldo pendiente |

No existe sync separado: entran en el mismo pull que facturas de venta publicadas con saldo.

---

## Campo `is_nota_debito` — ruta de datos

```
Odoo account.move.is_nota_debito
    ↓ HubAccountMove.fields_array
account_move.cs [Column("is_nota_debito")]
    ↓ AccountMoveDb.EnsureColumnAsync
SQLite account_move
    ↓ GetItemsWithBalanceByPartnerAsync
AccountPaymentCrud.LoadPaymentLinesForNew
    ↓ copia a línea
MultipleCobrosInvoiceLineAi.is_nota_debito
    ↓ propiedad calculada
document_reference_label
```

### Migración SQLite

`AccountMoveDb.Init()`:

```csharp
await EnsureColumnAsync("is_nota_debito", "INTEGER");
```

---

## Identificador visual

**Archivo:** `DMSA.Models.Odoo\Accounting\AccountMoveDocumentDisplay.cs`

```csharp
public const string InvoicePrefix = "FACT #";
public const string DebitNotePrefix = "NDBI";

public static string GetDocumentReferenceLabel(string? docnumMask, bool isNotaDebito)
{
    return isNotaDebito
        ? $"{DebitNotePrefix} {docnumMask}"
        : $"{InvoicePrefix} {docnumMask}";
}
```

| `is_nota_debito` | Etiqueta |
|------------------|----------|
| `false` | `FACT # 026-013-000001792` |
| `true` | `NDBI 026-013-000000123` |

Propiedad **`document_reference_label`** en:

- `account_move`
- `MultipleCobrosInvoiceLineAi`
- `AccountMoveSummary`

---

## Sync — Actualización de datos

### UI

`DMCobranzas\AppPages\UpdateData.xaml`:

- *Actualizar Facturas y Notas de Débito (Cabeceras)*
- *Actualizar Facturas y Notas de Débito (Detalles)*

### Servidor

`ServerPuller.Accounting.cs`:

- `[UpdateAction("Actualizar Facturas y Notas de Débito")]`
- `[UpdateAction("Actualizar Detalles de Facturas y Notas de Débito")]`

### Dominio sync cabeceras

`HubAccountMove.BuildHeaderDomain()`:

```csharp
new object[] { "move_type", "=", "out_invoice" },
new object[] { "state", "=", "posted" },
// is_nota_debito viene en fields, sin filtro extra
```

---

## Cobranzas — documentos aplicables

### Query con saldo

`AccountMoveDb.GetItemsWithBalanceByPartnerAsync()`:

```csharp
x.move_type == "out_invoice" && x.amount_residual > 0
```

Incluye **facturas y notas de débito** con saldo, orden `invoice_date ASC`.

### Copia al cargar facturas en cobro

`AccountPaymentCrud.xaml.cs` (aprox. líneas 947, 1215):

```csharp
is_nota_debito = accountMoveItem.is_nota_debito,
docnum_mask = accountMoveItem.docnum_mask,
```

### UI lista de aplicación

`MultipleCobrosInvoiceLineAiRow.xaml`:

```xml
<Label Text="{Binding document_reference_label}" ... />
```

---

## Popups de saldos

`PopupSelectInvoice.cs` — mensajes:

- *Se muestran facturas y notas de débito con saldo pendiente.*
- *20 documentos con saldo pendiente más antiguos (facturas y notas de débito)*
- EmptyView menciona facturas y notas de débito

---

## Impresión

Sección **DOCUMENTOS** del ticket recibo (`Processor.Template_MultipleCobrosInvoice()`):

```csharp
r.Line(doc.document_reference_label);
```

Al construir resumen desde `multiple_cobros_invoice_line_ai` se propaga `is_nota_debito`.

---

## Validaciones cruzadas

En alertas de exceso de saldo (`TryValidateAssignmentsBeforeSaveAsync`), el listado usa `document_reference_label` → el usuario distingue **FACT #** vs **NDBI**.

---

## Diagrama

```
Odoo                    SQLite              UI / Ticket
─────                   ──────              ───────────
out_invoice      →      account_move
is_nota_debito   →      is_nota_debito  →   NDBI {mask}
docnum_mask      →      docnum_mask     →   FACT # {mask}
amount_residual  →      amount_residual →   SALDO en cobro
```

---

## Plan de pruebas

| # | Caso | Esperado |
|---|------|----------|
| 1 | Sync actualización datos | ND en SQLite con `is_nota_debito=1` |
| 2 | Cobro cliente con ND pendiente | Lista muestra `NDBI …` |
| 3 | Cobro solo facturas | Lista muestra `FACT # …` |
| 4 | Popup Últimas 20 | Incluye ND con saldo |
| 5 | Ticket recibo con ND aplicada | DOCUMENTOS imprime `NDBI` |
| 6 | Alerta saldo excedido | Detalle muestra prefijo correcto |

---

*Informe Notas de Débito — Cobranzas Agosto 2026*

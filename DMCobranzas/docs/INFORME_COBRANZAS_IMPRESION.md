# Informe técnico — Impresión tickets Cobranzas

**Módulo:** DMCobranzas  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Fecha:** 25 de agosto de 2026  
**Proceso:** Ticket recibo individual y resumen de cierre de día

---

## Resumen ejecutivo

Se mejoró la **impresión del resumen de cobranzas** (grupo del día / cierre) para mostrar cantidades, números de comprobante/cheque por forma de pago, y se mantiene el ticket **recibo individual** con totales F/P, CANC., anticipo y documentos (`FACT #` / `NDBI`).

---

## Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `DMCobranzas\Services\Templates\Processor.cs` | Plantillas ticket |
| `DMCobranzas\Services\ReceiptBuilder.cs` | Formato ESC/POS y preview |
| `DMCobranzas\AppPages\Cobranzas\CobranzasPage.xaml.cs` | Acción TICKET → `TicketItem()` |
| `DMCobranzas\AppPages\PrintView` | Vista previa / impresión |
| `DMSA.Models.Odoo\StaticData\TipoEmision.cs` | Nombres tipo emisión |
| `DMSA.Models.Odoo\Accounting\AccountMoveDocumentDisplay.cs` | Etiquetas FACT # / NDBI |

---

## 1. Resumen de cobranzas (cierre de día)

### Cómo se imprime

Lista cobranzas → swipe **TICKET** sobre **grupo del día**  
→ `CobranzasPage.TicketItem()`  
→ `Processor.Template_MultipleCobrosInvoiceGroup(List<MultipleCobrosInvoice>)`

### Corrección de datos

Antes la agrupación copiaba solo `BankId` y `Amount` → faltaban `Circular` y `NumberCheckText` (salía `Tr#-`).

Ahora:

```csharp
Lines = g.ToArray()  // líneas completas desde SQLite
```

### Formato actual

```
MACRONEGOCIOS S.A.
Resumen Cobranzas
Dia: 2026-08-25
--------------------------------
Refer. Cierre: 12312312
Cant. Recibos: 10
Cobrador: …
--------------------------------
Transferencia              Cant. 5
BANCO CENTRAL DEL ECUADOR  -  N° 13212312213
BANCO DE GUAYAQUIL S.A.    -  N° 99887766
TOTAL Transferencia        $159,26
Efectivo                   Cant. 4
TOTAL Efectivo             $63,26
Cheque Dia                 Cant. 1
BANCO PICHINCHA            -  N° 0012345
TOTAL Cheque Dia           $150,00
--------------------------------
-Firma Vendedor-
```

### Detalle por tipo de emisión

| Tipo (`Type`) | Texto izquierdo | Número | Campo SQLite | Campo UI |
|---------------|-----------------|--------|--------------|----------|
| `transfer` | Banco | N° comprobante | `Circular` | `txtCircular` |
| `deposito` | Diario (`journal_name`) | N° comprobante | `Circular` | `txtCircular` |
| `check` / `check_day` | Banco | N° cheque | `NumberCheckText` | `txtNCheque` |
| Encabezado grupo | Nombre tipo | `Cant. {n}` | conteo líneas | — |

### Helpers (`Processor.cs`)

| Método | Resultado |
|--------|-----------|
| `FormatDocumentNumber(value)` | `N° {valor}` o `-` |
| `FormatSummaryDetailLine(label, num)` | `{label}  -  N° {num}` |
| `GetBankName(line, banks)` | Nombre desde `BankId` |

### Catálogo tipos

`DMSA.Models.Odoo\StaticData\TipoEmision.cs`: `transfer`, `deposito`, `cash`, `check_day`, `check`, `credit_card`

---

## 2. Ticket recibo individual

### Cómo se imprime

Lista cobranzas → **TICKET** sobre un recibo  
→ `Processor.Template_MultipleCobrosInvoice(MultipleCobrosInvoice)`

### Secciones principales

| Sección | Contenido |
|---------|-----------|
| Cabecera | Empresa, cliente, recibo, cobrador, estado |
| F.PAGO | Formas de pago con detalle por tipo |
| TOTAL F/P | Σ montos cobrados |
| DOCUMENTOS | Facturas / ND aplicadas (`document_reference_label`) |
| TOTAL CANC. | Σ `amount_asigned` |
| ANTICIPO | F/P − CANC. si > 0 |

### Formas de pago — detalle impreso

| Tipo | Líneas extra |
|------|--------------|
| `check` / `check_day` | F.Cobro, banco, `Ch# {NumberCheckText}` |
| `transfer` | Cta., `Dp# {Circular}`, diario |
| `deposito` | Cta., `Dp# {Circular}`, diario |
| `credit_card` | `Tj. Lote# {LoteVoucher}` |

### Documentos aplicados

```csharp
r.Line(doc.document_reference_label);
// FACT # 026-013-…  |  NDBI 026-013-…
```

Fuente snapshot: `multiple_cobros_invoice_line_ai` (`amount_residual`, `amount_asigned`).

### Anticipo en ticket

- Sin documentos y anticipo > 0 → *ANTICIPO DE CLIENTE* (o texto `Resumen` de la línea)
- Con documentos parciales → fila **ANTICIPO:** con monto diferencia

Cálculo:

```
totalCollected = Σ line.Amount
totalAmountApplied = Σ amount_asigned
anticipoAmount = totalCollected - totalAmountApplied  (si > 0)
```

---

## 3. ReceiptBuilder

**Ruta:** `DMCobranzas\Services\ReceiptBuilder.cs`

| Método | Uso |
|--------|-----|
| `Columns(left, right)` | Línea 32 chars impresora |
| `Line(text)` | Línea simple |
| `Separator()` | Guiones 32 chars |
| `Build()` / `BuildPreview()` / `BuildPreviewHtml()` | Salidas |

---

## Plan de pruebas

| # | Caso | Esperado |
|---|------|----------|
| 1 | Ticket resumen transferencias | `Cant. n` + banco `- N° comprobante` |
| 2 | Ticket resumen cheques | Banco `- N° cheque` |
| 3 | Ticket resumen depósitos | Diario `- N° comprobante` |
| 4 | Recibo $50 / $25 aplicado | TOTAL CANC. $25, ANTICIPO $25 |
| 5 | Recibo con ND | DOCUMENTOS muestra `NDBI …` |
| 6 | Recibo anticipo puro | ANTICIPO DE CLIENTE |

---

*Informe Impresión — Cobranzas Agosto 2026*

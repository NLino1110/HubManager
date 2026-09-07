# Informe técnico — Alertas y validaciones UI (Cobranzas)

**Módulo:** DMCobranzas  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Fecha:** 25 de agosto de 2026  
**Proceso:** Alertas, confirmaciones, loading y reglas de estado (editar / enviar)

---

## Resumen ejecutivo

Se unificaron mensajes en **`DisplayAlertAsync`** (modal) en lugar de toasts donde correspondía, se agregaron **validaciones al guardar** cobros con alertas detalladas, se definieron reglas de **edición y sincronización** según `payment_status`, y se incorporó **loading** al descargar facturas del cliente tras confirmación.

---

## Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `DMCobranzas\AppPages\Cobranzas\CobranzasPage.xaml.cs` | Alert día cerrado; validación EDITAR / ENVIAR |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentView.xaml.cs` | Validación edición recibo; loading descarga |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentCrud.xaml.cs` | Alertas saldo / total aplicado (ver informe Anticipos) |
| `DMSA.Models.Odoo\DebitCollection\CobrosEstados.cs` | `CanEdit()` / `CanSync()` |
| `DMCobranzas\Controls\CustomRows\MultipleCobrosInvoiceRow.xaml` | Visibilidad botones según estado |
| `DMSA.Sync.Core\Update\Pusher\DebitCollection.cs` | Rechazo sync en pusher |
| `DMCobranzas\MainPage.xaml.cs` | Auto-envío solo PENDIENTE |
| `DMCobranzas\Settings\helpers\UITools.cs` | Popup loading |

---

## 1. Día cerrado — nuevo cobro bloqueado

### Pantalla / método

`CobranzasPage.xaml.cs` → `NewPayment()`

### Antes

Toast de Windows (fácil de pasar por alto).

### Después

| Campo | Valor |
|-------|-------|
| Método | `DisplayAlertAsync` |
| Título | Atención |
| Mensaje | Ya se ha cerrado el día, no podrá ingresar más cobros hasta iniciar un nuevo período. |
| Botón | Aceptar |

### Validación

```csharp
AccountPaymentDailyDb.GetItemAsync(company_id, DateTime.Now.ToString("yyyy-MM-dd"))
```

Si existe registro de cierre → no abre `AccountPaymentView`.

---

## 2. Estados — editar vs. sincronizar

### Modelo

**Archivo:** `DMSA.Models.Odoo\DebitCollection\CobrosEstados.cs`

| Constante | Significado |
|-----------|-------------|
| `PENDIENTE` | Cobro local, pendiente de envío |
| `ENVIANDO` | Sync en curso |
| `ERROR` | Falló envío a Odoo |
| `PROCESADO` / `APLICADO` / `CANCELADO` | Finales |

### Reglas

```csharp
CanEdit(status)  → PENDIENTE o ERROR
CanSync(status)  → solo PENDIENTE
```

### Matriz de acciones

| Estado | EDITAR | ENVIAR | TICKET |
|--------|--------|--------|--------|
| PENDIENTE | ✓ | ✓ | ✓ |
| ERROR | ✓ | ✗ | ✓ |
| PROCESADO / APLICADO / CANCELADO | ✗ | ✗ | ✓ |

### Rutas de validación

| Acción | Archivo | Método |
|--------|---------|--------|
| Botón EDITAR (lista) | `CobranzasPage.xaml.cs` | `EditItem()` |
| Botón ENVIAR (lista) | `CobranzasPage.xaml.cs` | `EnviarCobro()` |
| Swipe / WinUI | `MultipleCobrosInvoiceRow.xaml` | DataTriggers |
| Edición recibo | `AccountPaymentView.xaml.cs` | `EnsureReceiptIsEditableAsync()` |
| Push Odoo | `DebitCollection.cs` | `SendPayment()` |
| Auto-envío login | `MainPage.xaml.cs` | `LoadSession()` |

### Textos de alerta

| Situación | Mensaje |
|-----------|---------|
| Editar bloqueado | *Solo se puede editar cobros en estado PENDIENTE o ERROR. Estado actual: {estado}.* |
| Enviar bloqueado | *Solo se puede sincronizar cobros en estado PENDIENTE. Estado actual: {estado}.* |
| Recibo no editable | *Este cobro no se puede editar porque no está en estado PENDIENTE o ERROR.* |

### Flujo ERROR → reenvío

1. Envío falla → `payment_status = ERROR`
2. Usuario edita y guarda → vuelve a `PENDIENTE`
3. Usuario **ENVIAR** → sync permitida

---

## 3. Validaciones al guardar línea de cobro (saldo)

**Archivo:** `AccountPaymentCrud.xaml.cs`  
**Método:** `TryValidateAssignmentsBeforeSaveAsync()`

| Validación | Alerta |
|------------|--------|
| `amount_asigned > amount_residual` | Lista por documento con `document_reference_label` |
| `totalAssigned > totalPayment` | Total aplicado supera monto del cobro |

Detalle de mensajes en informe **Anticipos** (comparte flujo de guardado).

---

## 4. Loading — descarga facturas del cliente

### UI

`AccountPaymentView.xaml` — botón azul descarga (`btnDownloadData`, icono `&#xf019;`)

### Flujo

```
Click
  → DisplayAlertAsync: "Obtener actualizaciones de facturas. ¿Desea continuar?"
  → Si confirma:
       ShowLoadingPopup("Actualizando facturas del cliente...")
       ServerPuller sync partner
       Toast "Terminado."
       finally HideLoadingPopup()
```

**Método:** `AccountPaymentView.xaml.cs` → `DownloadDataCustomer()`

---

## 5. Otros alertas existentes en cobranzas (referencia)

| Pantalla | Título | Cuándo |
|----------|--------|--------|
| `AccountPaymentCrud` | Distribución automática | Usuario limpia asignación a 0 |
| `AccountPaymentCrud` | Atención | Excede máximo permitido en fila |
| `AccountPaymentCrud` | Atención | Cheque PF con fecha ≤ hoy |
| `AccountPaymentCrud` | Atención | Tarjeta sin Bin/Lote |
| `CobranzasPage` | Cierre no permitido | Cierre día con cobros pendientes |
| `CobranzasPage` | Envío de cobro | Confirmación antes de ENVIAR |

---

## Plan de pruebas

| # | Caso | Esperado |
|---|------|----------|
| 1 | Día cerrado → Nuevo cobro | Modal Atención, no abre cobro |
| 2 | Cobro PROCESADO → EDITAR | Alerta; botón oculto |
| 3 | Cobro ERROR → EDITAR | Permite; guardar → PENDIENTE |
| 4 | Cobro ERROR → ENVIAR | Alerta bloqueo |
| 5 | Descarga facturas + confirmar | Loading hasta fin |
| 6 | Aplicar > SALDO | Alerta con FACT # / NDBI |

---

*Informe Alertas — Cobranzas Agosto 2026*

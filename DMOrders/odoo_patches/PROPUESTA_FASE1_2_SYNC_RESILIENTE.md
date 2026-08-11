# Propuesta Fase 1 + 2 — dónde irían los cambios (SIN aplicar)

Documento de diseño. **No modifica** el código actual de DMOrders ni Odoo.
Sirve solo para ubicar puntos de inserción.

---

## Mapa rápido

| Fase | Archivo | Qué se agregaría (nuevo / alrededor) |
|------|---------|--------------------------------------|
| 1 | `DMOrders/MainPageTab.xaml.cs` → `SendFullData` | Check de red **antes** del loading; resumen OK/FAIL al final |
| 1 | `DMSA.Sync.Core/Update/Pusher/SaleOrders.cs` → `SendSaleOrder` / `SendAllSaleOrders` | try/catch red; resultado estructurado |
| 1 | (opcional nuevo) clase `SaleOrderSyncResult` | DTO de resultado por pedido |
| 2 | `ApiManagerOdoo/Sale/HubSaleOrder.cs` (o `.Extend.cs`) | Método **nuevo** `GetByExternalGuid` |
| 2 | `SaleOrders.cs` → `SendSaleOrder` | Antes/después del `Create`: recuperar por GUID si ya existe |
| 2 | `odoo_patches/sale_order_extend.py` → `create` | Si GUID ya existe → devolver esa orden (idempotente) |

---

## FASE 1 — Red + resumen

### 1.A `MainPageTab.xaml.cs` — método `SendFullData` (~línea 122)

**Hoy (no se cambia aún):**

```csharp
await UITools.ShowLoadingPopup(this);
var syncedOrders = await serverPusher.SendAllSaleOrders();
// ... alert solo con OK
```

**Dónde insertar (propuesta):**

```text
SendFullData()
  │
  ├─ [NUEVO] if (!HayConexionOdoo()) → DisplayAlert + return   ← ANTES del ShowLoadingPopup
  ├─ (igual) SendAllSaleOrders / tasks / SyncSaleOrders
  ├─ (igual) ReloadData + SelectTab("orders")
  └─ [CAMBIO] alert con OK + FALLIDOS (no solo lista de OK)
```

Ejemplo de helper **nuevo** (mismo archivo o `Controls/Tools`):

```csharp
// PROPUESTA — método nuevo, no existe hoy
private bool HayConexionOdoo()
{
    // Connectivity.Current.NetworkAccess == NetworkAccess.Internet
    // y/o ping corto al Host de odooConnection
    return true;
}
```

---

### 1.B `SaleOrders.cs` — `SendSaleOrder` (~línea 127)

**Hoy:**

```csharp
ApiResponseOdooRpcT<int> resultTask = await hubSaleOrder.Create(...);
if (resultTask?.error != null) { Toast...; return false; }
if (resultTask?.result > 0) { is_synchronized = true; ... }
```

**Dónde envolver (propuesta):**

```text
SendSaleOrder()
  │
  ├─ (igual) external_guid, PreparePayLoad, RemoveGiftLines
  ├─ [NUEVO] try {
  │            resultTask = await Create(...)
  │          } catch (HttpRequestException / TaskCanceledException / ...) {
  │            return Failed("Sin conexion / timeout")
  │          }
  ├─ (igual) si error RPC → Failed con mensaje Odoo
  └─ (igual) si OK → marcar sync
```

---

### 1.C `SaleOrders.cs` — `SendAllSaleOrders` (~línea 316)

**Hoy:** `Task<List<string>>` solo con labels OK.

**Propuesta (clase nueva, archivo nuevo opcional):**

```csharp
// PROPUESTA — DMSA.Sync.Core/Update/Pusher/SaleOrderSyncResult.cs (archivo NUEVO)
public class SaleOrderSyncResult
{
    public string IdReferencia { get; set; }
    public string ExternalGuid { get; set; }
    public bool Ok { get; set; }
    public string ErpName { get; set; }
    public string ErrorMessage { get; set; }
}
```

```text
SendAllSaleOrders()
  └─ por cada pedido:
       result = SendSaleOrder(...)
       lista.Add(result)   // OK o Failed
  return lista completa
```

`MainPageTab` armaría:

```text
OK: PED-001, PED-002
Fallidos: M007-... → Sin conexion
```

---

## FASE 2 — Idempotencia `external_guid`

### 2.A API — método **nuevo** (no rompe Create actual)

**Archivo:** `ApiManagerOdoo/Sale/HubSaleOrder.cs` o `HubSaleOrder.Extend.cs`

```csharp
// PROPUESTA — método NUEVO
public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByExternalGuid(string externalGuid)
{
    // SearchRead: domain [("external_guid", "=", externalGuid)]
    // fields: id, name, external_guid, ...
}
```

Igual patrón que el `GetById` que ya tienes (~línea 43).

---

### 2.B `SendSaleOrder` — flujo propuesto (alrededor del Create)

```text
                    [hoy]
PreparePayLoad → Create → si OK marcar sync

                    [propuesta Fase 2]
PreparePayLoad
     │
     ├─ [NUEVO] existing = GetByExternalGuid(guid)
     │            si existe → erp_id/name, is_synchronized=true, return Ok
     │            (evita create duplicado si el primer intento sí llegó a Odoo)
     │
     ├─ Create(...)
     │
     ├─ si error contiene "External GUID" / unique:
     │      [NUEVO] existing = GetByExternalGuid(guid) → recuperar y Ok
     │
     └─ si OK → marcar sync (igual que hoy)
```

**Importante:** el `Create` actual **no se elimina**; solo se rodea con lookup/reintento.

---

### 2.C Odoo — `sale_order_extend.py` → `create` (~línea 37)

**Hoy:**

```python
orders = super(...).create(vals_list)
for order in orders:
    if order.id_referencia and order.mobile_sync:
        order._procesos_especiales_externa()
return orders
```

**Propuesta (antes del `super().create`):**

```text
create(vals_list)
  │
  ├─ [NUEVO] para cada vals:
  │     guid = vals.get("external_guid")
  │     si guid y ya existe en BD:
  │         agregar esa orden a "ya_existentes"
  │         quitar ese vals de la lista a crear
  │
  ├─ (igual) super().create(vals_restantes)
  │
  ├─ (igual) _procesos_especiales_externa solo en las NUEVAS
  │     (las ya existentes: no reaplicar promos a ciegas, o flag aparte)
  │
  └─ return ya_existentes + nuevas
```

Así el create es **idempotente** sin borrar tu lógica de promos.

---

## Qué NO se tocaría en Fase 1+2

- `Crud.xaml.save.cs` / sequences / promociones viewer  
- Motor de promos / `Tools.v2`  
- Wizard completo de Odoo (solo el `create` idempotente)  
- Cola `sync_status` / reintentos automáticos (eso sería Fase 4)

---

## Orden de implementación sugerido (cuando lo apruebes)

1. Helper red + try/catch en `SendSaleOrder` (Fase 1)  
2. `SaleOrderSyncResult` + resumen en `SendFullData` (Fase 1)  
3. `GetByExternalGuid` en Hub (Fase 2)  
4. Lookup antes/después Create (Fase 2)  
5. Idempotencia en `create` Python (Fase 2)

---

*Archivo solo documental. Código productivo intacto.*

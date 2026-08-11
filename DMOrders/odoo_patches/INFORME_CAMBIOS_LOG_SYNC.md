# Informe: logs de sincronización DMOrders

Fecha: 2026-08-05  
Alcance: traza `[DMOrders Sync]` en Output de Visual Studio / Debug (no es el `_logger` de Odoo).

---

## 1. Resumen ejecutivo

| Problema | Causa | Solución |
|----------|--------|----------|
| Spam de `[DMOrders Sync] RPC \| res.partner.search_read` | Se escribía en **cada** `SearchRead` (paginación) | Se eliminó el log RPC por petición |
| No se veía claramente inicio / fallo / fin de una fase | Solo había trazas sueltas o RPC | Flujo fijo: **INICIO → INGRESO AL PROCESO → (FALLO) → FIN** |
| Errores de sync poco visibles en Output | Toast sí, log inconsistente | `FALLO` con tipo + mensaje; `FIN \| FALLO` |

---

## 2. Antes / después

### 2.1 Log por RPC

**Antes**

- En `HubBase.SearchRead`, cada llamada imprimía:
  - `[DMOrders Sync] RPC | res.partner.search_read`
- Con clientes paginados (p. ej. 50 páginas) → el mismo aviso se repetía decenas de veces.

**Después**

- Sin log RPC por petición.
- Un aviso por **fase de sync** (Clientes, Productos, Stock, etc.), no por cada HTTP a Odoo.
- El POST `call_kw` sigue viéndose en el log de Odoo si se necesita depurar el servidor.

### 2.2 Flujo por fase (éxito)

```
[DMOrders Sync] INICIO | Clientes | 2026-08-05 ...
[DMOrders Sync] INGRESO AL PROCESO | Clientes | 2026-08-05 ...
[DMOrders Sync] FIN | Clientes | OK | 12345 ms | 2026-08-05 ...
```

### 2.3 Flujo por fase (error)

```
[DMOrders Sync] INICIO | Clientes | ...
[DMOrders Sync] INGRESO AL PROCESO | Clientes | ...
[DMOrders Sync] FALLO | Clientes | ExceptionType: mensaje...
[DMOrders Sync] FIN | Clientes | FALLO | 890 ms | ...
```

En actualización manual compuesta, el bloque externo `ActualizacionManual` también marca `FALLO` si una fase relanza la excepción.

### 2.4 Sync al arranque / LaunchManager

- `SafeExecute` usa `RunAsync(..., swallowErrors: true)`.
- Si falla: queda el `FALLO` en Output y el Toast al usuario; las demás fases pueden continuar.

---

## 3. Archivos modificados

Rutas desde `HubManager\`.

| Archivo | Rol | Cambio |
|---------|-----|--------|
| `DMSA.Sync.Core\Update\SyncActivityLog.cs` | Utilidad de log | `RunAsync` con INICIO/INGRESO/FALLO/FIN; `Begin` + `MarkFailed` |
| `ApiManagerOdoo\Base\HubBase.cs` | RPC Odoo | Quitado `Debug.WriteLine` RPC por cada `search_read` |
| `DMOrders\Services\Update\LaunchManager.cs` | Sync al login / arranque | `SafeExecute` → `RunAsync` + toast si error |
| `DMOrders\Pages\Sys\UpdateData.xaml.cs` | Actualización manual | Fases con `RunAsync`; scope externo con `MarkFailed` en catch |

---

## 4. Cómo filtrar en Visual Studio

En la ventana **Output**, filtrar o buscar:

`[DMOrders Sync]`

Fases típicas: `Promociones`, `CatalogoMaestros`, `Clientes`, `Productos`, `ListasPrecios`, `Stock`, `Pedidos`, `CatalogoImagenes`, `ActualizacionManual`.

---

## 5. Qué no cambia

- Dominios / queries a Odoo (misma lógica de sync).
- Logs de Odoo en el servidor (`_logger` / `call_kw`).
- Toast de error en sync de arranque (`LaunchManager`).

---

## 6. Criterio de aceptación

- [ ] Al sincronizar clientes **no** aparece una línea RPC por página.
- [ ] Por cada fase aparece como máximo el bloque INICIO / INGRESO / FIN (y FALLO solo si hay error).
- [ ] Ante error de red o de Odoo en una fase, Output muestra `FALLO` y `FIN | FALLO`.

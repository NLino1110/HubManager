# Informe: cambios DMCobranzas — sesión Septiembre 2026

**Fecha de consolidación:** 10 de septiembre de 2026  
**Módulo:** DMCobranzas (+ DMSA.Sync.Core, ApiManagerOdoo, DMSA.Models.Odoo)  
**Ruta canónica:** `D:\src_r00t\apps\stables\.net\10\HubManager`  
**Commit:** `0235058` — rama `fix/dev-njll-DMOrders` (merge fast-forward a `main` en GitHub)

---

## 1. Resumen ejecutivo

| Área | Problema | Motivo de la mejora | Resultado |
|------|----------|---------------------|-----------|
| **Notas de crédito — detalle** | Filas ilegibles en móvil; UOM oculta; sin alerta de cantidad inválida | Operadores en campo necesitan ver conversión UOM y validar devoluciones sin errores silenciosos | Layout compacto en columnas; `Cant.xU.M` visible; alerta si devolver > disponible |
| **Notas de crédito — CRUD / grupo** | Guardar/agregar sin validar cliente, factura o productos; nombres vacíos en listado; UI no responsive | Evitar registros incompletos en Odoo y mejorar UX en teléfono | Alertas en guardar/agregar; bloqueo parcial en edición; botones centrados en móvil |
| **Popups de búsqueda** | Botón Buscar se solapaba / bajaba demasiado en teléfono | Búsqueda de cliente/producto/factura inutilizable en pantallas pequeñas | Layout compacto por filas; Buscar debajo del campo con espaciado controlado |
| **Cobros — cabecera** | Botones 🔍/✕ sobre el campo; AGREGAR/Saldos desordenados en móvil | Misma pantalla de selección de cliente que NC, misma falla de layout | Campo limpio + barra de acciones responsive y centrada en teléfono |
| **Login / Home** | Regresar cortado en móvil; última sync global entre conexiones | Usuario cambia Dmujeres ↔ Macro y ve fecha de sync incorrecta | Botones apilados en narrow; sync por `DbNameSqlite` |
| **Tema del dispositivo** | Modo oscuro del SO alteraba textos/colores | App diseñada para tema claro | `UserAppTheme = Light` + configuración nativa Android/iOS |
| **Sync UOM / Odoo** | Falta catálogo UOM local para NC; campos incompletos en pull | Alinear app con Odoo (`uom_uom`, líneas factura, partners) | Sync en “Otros”; helpers y APIs extendidas |

**Alcance:** 45 archivos, +2240 / −1384 líneas. **No afecta DMOrders** (sync UOM aditivo en rama Cobranzas).

---

## 2. Motivo general de las mejoras

La sesión se centró en **usabilidad móvil** y **corrección funcional** del flujo de **Notas de crédito** y **Cobros**, donde los usuarios reportaron:

1. Controles que se pierden o se solapan en teléfono vertical.
2. Validaciones insuficientes al armar solicitudes de NC.
3. Popups de búsqueda con botón Buscar mal posicionado.
4. Etiqueta “Ult. sincronización” que no reflejaba la conexión activa (Dmujeres vs Macro).
5. Modo oscuro del dispositivo distorsionando la UI.

---

## 3. Antes / después por flujo

### 3.1 Popup — búsqueda cliente / producto

| | Descripción |
|---|-------------|
| **Antes** | Botón Buscar en fila/toolbox separada del grid; al asignarse ~500 ms después en `_OnAppearing`, invadía la barra gris “Productos” / “Datos de Cliente”. En móvil, hueco excesivo entre campo y botón. |
| **Error** | Botón ilegible, solapado o demasiado abajo; búsqueda difícil en teléfono. |
| **Después** | `BuildCompactTopBar`: título + campo en fila 0; Buscar en `_compactSearchSection` (6 px bajo el Entry). `UseCompactPopupTopLayout()` solo `DeviceIdiom.Phone`. Popups Partner/Product/Invoice alineados (Invoice sin botón Buscar; acciones en toolbox). |

### 3.2 NC — fila de detalle de producto

| | Descripción |
|---|-------------|
| **Antes** | Layout poco compacto; conversión UOM condicionada; cantidad > disponible sin feedback claro en `ContentView`. |
| **Error** | Usuario no veía factor UOM; podía ingresar cantidades inválidas. |
| **Después** | Columnas: Nº/fecha, producto, precio, % desc, Cant. Disp., Cant.xU.M., Devolver, acciones. Alerta vía `GetHostPage().DisplayAlertAsync`. Helper `CreditNoteUomDisplayHelper`. |

### 3.3 NC — grupo (cliente + acciones)

| | Descripción |
|---|-------------|
| **Antes** | Botones en fila fija; en móvil alineados a la izquierda o fuera de pantalla; 🔍/✕ transparentes (invisibles). |
| **Error** | Difícil pulsar buscar/limpiar/agregar en teléfono. |
| **Después** | `VisualStateManager` Narrow/Wide (720 px): móvil → acciones en fila 1 **centradas**; tablet → misma fila que cliente a la derecha. Estilos `IconSearchButtonStyle` (DodgerBlue) e `IconClearButtonStyle` (IndianRed). |

### 3.4 Cobros — cabecera (`AccountPaymentView`)

| | Descripción |
|---|-------------|
| **Antes** | 🔍/✕ dentro del `Entry` (`WidthRequest="2"` en búsqueda); AGREGAR/Saldos/Descarga en columnas distintas rotas en narrow. |
| **Error** | Iconos sobre el placeholder; AGREGAR parcialmente visible. |
| **Después** | Mismo patrón que NC: `GridPartner` solo Entry; `StackActions` con 5 botones; narrow centrado, wide alineado a la derecha. |

### 3.5 Login — agencia Ingresar / Regresar

| | Descripción |
|---|-------------|
| **Antes** | `HorizontalStackLayout` con dos botones `WidthRequest="250"` → ~500 px; Regresar cortado en teléfono. |
| **Error** | Solo visible “Ingresar”. |
| **Después** | `AgencyActionsGrid` + `AdaptiveTrigger`: narrow apilados; wide lado a lado. |

### 3.6 Ult. sincronización por conexión

| | Descripción |
|---|-------------|
| **Antes** | Clave global `Preferences["last_log_fec_sincro"]`; al cambiar compañía en login seguía mostrando sync de la conexión anterior. |
| **Error** | Sync en Dmujeres visible al seleccionar Macro. |
| **Después** | `SyncStatusLabels`: clave `last_log_fec_sincro_{DbNameSqlite}`; resolución prioriza SQLite de la conexión activa; refresh en `OnConnectionSelectedAsync`. |

### 3.7 Tema claro forzado

| | Descripción |
|---|-------------|
| **Antes** | Tema seguía al SO; `Styles.xaml` con `AppThemeBinding` oscuro; login forzaba Light solo en `OnAppearing`. |
| **Error** | Textos/ fondos ilegibles con modo oscuro del teléfono. |
| **Después** | `App.xaml.cs` → `UserAppTheme = Light`; Android `Theme.MaterialComponents.Light` + `forceDarkAllowed=false`; iOS `UIUserInterfaceStyle=Light`. |

---

## 4. Archivos modificados

Rutas relativas desde `HubManager\`.

| Archivo | Pantalla / componente | Motivo | Antes → Después (resumen) |
|---------|----------------------|--------|---------------------------|
| `DMSA.Sync.Core\Controls\Popups\PopupSelectBase.cs` | Base popups selección | Layout móvil Buscar | Toolbox en grid suelto → `_compactSearchSection` + toolbar móvil; helpers `CreateSearchButton`, `ApplyMobileToolbarContent` |
| `DMSA.Sync.Core\Controls\Popups\PopupSelectPartner.cs` | Popup clientes | UX búsqueda | Leyenda 2 chars, empty view, `WrapSearchButton` |
| `DMSA.Sync.Core\Controls\Popups\PopupSelectProductInMove.cs` | Popup productos NC | Idem clientes | Mismo patrón + altura CollectionView móvil |
| `DMSA.Sync.Core\Controls\Popups\PopupSelectInvoice.cs` | Popup facturas | Acciones sin Buscar | Últimas 20 / Detalle general en toolbox; sin botón Buscar duplicado |
| `DMSA.Sync.Core\Controls\Popups\PopupSize.cs` | Tamaño popup | Móvil | ~98% ancho × 92% alto en phone |
| `DMCobranzas\Controls\CustomRows\CreditNoteRequestDetailRow.xaml` | Fila detalle NC | Legibilidad móvil | Grid multi-columna compacto |
| `DMCobranzas\Controls\CustomRows\CreditNoteRequestDetailRow.xaml.cs` | Fila detalle NC | Validación UOM/qty | Alerta cantidad; binding UOM helper |
| `DMCobranzas\Services\CreditNoteUomDisplayHelper.cs` | **Nuevo** | Display UOM | Texto conversión cantidad × factor |
| `DMCobranzas\Settings\helpers\CreditNoteUiSettings.cs` | **Nuevo** | Flags UI NC | Configuración visual NC |
| `DMCobranzas\AppPages\NotaCredito\CreditNoteRequestCrud.xaml` | CRUD NC | UI | Ajustes layout campos |
| `DMCobranzas\AppPages\NotaCredito\CreditNoteRequestCrud.xaml.cs` | CRUD NC | Validaciones | Alertas módulo/tipo/cliente/factura/productos; nombres display |
| `DMCobranzas\AppPages\NotaCredito\CreditNoteRequestGroupView.xaml` | Grupo NC | Responsive + botones | VisualState Narrow/Wide; iconos con color |
| `DMCobranzas\AppPages\NotaCredito\CreditNoteRequestGroupView.xaml.cs` | Grupo NC | Lock edición | Cliente/búsqueda bloqueados; Agregar habilitado; alertas |
| `DMCobranzas\AppPages\Cobranzas\AccountPaymentView.xaml` | Cobro — cabecera | Responsive | StackActions unificado; iconos azul/rojo |
| `DMCobranzas\Login.xaml` | Login — agencia | Botones narrow | Grid Ingresar/Regresar apilados |
| `DMCobranzas\Login.xaml.cs` | Login | Sync label | `RefreshLastSyncLabelAsync` por conexión |
| `DMCobranzas\Services\SyncStatusLabels.cs` | **Nuevo** | Sync por conexión | Preferencias y resolución por `DbNameSqlite` |
| `DMCobranzas\AppPages\UpdateData.xaml.cs` | Actualizar datos | Persist sync | `PersistLastSyncDate(..., odooConnection)` |
| `DMCobranzas\App.xaml.cs` | App | Tema | `UserAppTheme = Light` al inicio |
| `DMCobranzas\Platforms\Android\Resources\values\styles.xml` | Android nativo | Tema | DayNight → Light; `forceDarkAllowed=false` |
| `DMCobranzas\Platforms\iOS\Info.plist` | iOS | Tema | `UIUserInterfaceStyle=Light` |
| `DMCobranzas\Platforms\MacCatalyst\Info.plist` | Mac Catalyst | Tema | Idem iOS |
| `DMCobranzas\Resources\Styles\Styles.xaml` | Estilos globales | Botones icono | `IconSearchButtonStyle`, `IconClearButtonStyle` |
| `DMCobranzas\MainPage.xaml.cs` | Home | Etiqueta sync | Usa `SyncStatusLabels` |
| `DMCobranzas\Services\AppTools.cs` | Utilidades | APK update date | Soporte label actualización |
| `ApiManagerOdoo\HubUomUom.cs` | API Odoo UOM | Sync NC | GetAll / count extendidos |
| `ApiManagerOdoo\HubResPartner.cs` | API partners | Búsqueda popup | Campos/consultas ampliadas |
| `ApiManagerOdoo\Accounting\HubAccountMove.cs` | API facturas | NC | Ajustes lectura moves |
| `ApiManagerOdoo\Accounting\HubAccountMoveLine.cs` | API líneas factura | Productos NC | Campos UOM/cantidades |
| `DMSA.Models.Odoo\Inventory\uom_uom.cs` | Modelo UOM | Sync | `category_id` y metadatos |
| `DMSA.Models.Odoo\Accounting\credit_note_request_detail.cs` | Modelo detalle NC | UOM/qty | Campos alineados Odoo |
| `DMSA.Sync.Core\Database\Sqlite\UomUomDb.cs` | SQLite UOM | Persistencia | CRUD sync local |
| `DMSA.Sync.Core\Database\Sqlite\ResPartnerDb.cs` | SQLite partners | Búsqueda popup | `GetItemsBySearchAsync` mejorado |
| `DMSA.Sync.Core\Database\Sqlite\Payments\AccountMoveDb.cs` | SQLite facturas | Filtros NC | Consultas factura/líneas |
| `DMSA.Sync.Core\Update\ServerPuller.cs` | Pull general | Orquestación | Hook sync UOM en “Otros” |
| `DMSA.Sync.Core\Update\ServerPuller.Accounting.cs` | Pull contabilidad | NC/facturas | Rangos y campos |
| `DMSA.Sync.Core\Update\ServerPuller.ResPartner.cs` | Pull partners | Clientes | Incremental |
| `DMSA.Sync.Core\Update\ServerPuller.StockQuant.cs` | Pull inventario | Stock NC | Ajustes |
| `DMCobranzas\AppPages\Sys\Connections.xaml` | Conexiones | UI | Refactor layout |
| `DMCobranzas\AppPages\UpdateData.xaml` | Actualizar datos | UI | Refactor layout |
| `DMCobranzas\AppPages\Cobranzas\CobranzasPage.xaml` | Listado cobros | UI menor | Ajustes menores |
| `DMCobranzas\AppPages\NotaCredito\NotasCreditoPage.xaml` | Listado NC | UI menor | Ajustes menores |
| `DMCobranzas\DMCobranzas.csproj` | Proyecto | Referencias | Includes nuevos helpers |

**No incluido en commit:** `DMCobranzas\Controls\CustomRows\CreditNoteRequestDetailRow.Legacy.xaml.txt` (respaldo local).

---

## 5. Git / despliegue

| Remoto | Estado |
|--------|--------|
| **GitHub** (`github`) | Rama `fix/dev-njll-DMOrders` y `main` en `0235058` |
| **Azure DevOps** (`origin`, VS 2022) | Pendiente push con credenciales locales |

```powershell
cd D:\src_r00t\apps\stables\.net\10\HubManager
git push origin fix/dev-njll-DMOrders
git push origin main
```

Recompilar **DMCobranzas** desde `10\HubManager`. Cerrar app/VS si DLLs bloqueadas.

---

## 6. Cómo probar

1. **Teléfono vertical — popup productos:** escribir 2+ chars → Buscar visible bajo campo, sin tapar header.
2. **NC grupo:** cambiar ancho &lt; 720 px → 🔍 ✕ Agregar centrados; ≥ 720 px → a la derecha del cliente.
3. **Cobros:** mismo criterio; campo cliente sin iconos encima.
4. **Login agencia:** Ingresar y Regresar completos en móvil.
5. **Sync por conexión:** sync en Dmujeres → login Macro → etiqueta distinta o “-”.
6. **Modo oscuro SO:** app permanece en tema claro.
7. **NC detalle:** línea con UOM caja×unidad; alerta si devolver &gt; disponible.

---

## 7. Sync admin — ZIP diario de documentos (16-sep-2026)

**Motivo:** el admin (grupo Mobile App 218) baja cientos de miles de cabeceras/detalle por `search_read` paginado. En Odoo el cron de `mnsa.mobile.document.pack` (03:00) ya arma un ZIP del último año. La tablet no lo consumía.

**Quién:** solo **admin Mobile App** (`IsMobileAppAdmin`). Vendedor: sin cambios (cartera + HTTP).

**Flag local:** `user_access.is_mobile_app_admin` guarda **0** (no) o **1** (sí). Odoo `sel_groups_218` trae **218** si es admin; no se persiste ese id.

**Cuándo:** **primera sync del día** (`IsFirstAccountMoveSyncOfDay`). Reintento (resume cabecera/detalle): no usa ZIP. Segunda sync del mismo día: incremental HTTP como antes.

**Fechas UI (admin, 1ra del día):** Desde = hoy − 1 año, Hasta = hoy. Vendedor 1ra del día sigue en 6 meses. El resto del día: ambos solo hoy.

**Cómo (tablet):**

1. `search_read` `mnsa.mobile.document.pack` (`state=done`, con adjunto), el más reciente.
2. Descarga `/web/content/{attachment_id}?download=true` **a disco** (no RAM).
3. Descomprime `account_move.json` + `account_move_line.json` + `manifest.json`.
4. Inserta en SQLite por lotes de 2000 (`OR REPLACE`).
5. Si no hay pack / falla descarga: **fallback** al sync HTTP actual.

**No** llama `action_request_document_pack` (generación síncrona, timeout). El ZIP lo genera el cron.

**Flag:** `ServerPuller.EnableAdminDocumentPackZipSync = true` (consumo ZIP en producción).

**Pruebas (oculto):** `ServerPuller.EnableForceFirstSyncOfDayUi = false`. Si se pone `true`, Actualizar datos muestra “Forzar 1ra sync del día”: borra `account_move_sync_day_*` y permite volver a bajar el ZIP de Odoo (cabecera + detalle) el mismo día, sin borrar facturas ni `log_fec_sincro`. No activar en producción.

**Campos (HTTP = ZIP):** una sola lista `AccountDocumentSyncFields` (C#) y `HEADER_FIELDS` / `LINE_FIELDS` (Odoo). El ZIP anterior no pedía `docnum_mask`, `partner_sale_id`, `pf_promised_amount`, `is_nota_debito` (cabecera) ni `quantity_available`, `discount_balance`, `analytic_line_ids` (detalle); al leer quedaban en blanco. Al abrir el JSON se validan las claves del primer objeto; si faltan, se descarta el ZIP y se usa HTTP. Tras actualizar `mnsa_mobile` hay que regenerar el paquete.

| Archivo | Cambio |
|---------|--------|
| `ApiManagerOdoo/Base/HubBase.cs` | `DownloadToFileAsync` (stream, timeout 3 h) |
| `ApiManagerOdoo/Specials/HubMnsaMobileDocumentPack.cs` | Último pack done + descarga adjunto |
| `DMSA.Models.Odoo/Specials/mnsa_mobile_document_pack.cs` | Modelo del pack |
| `DMSA.Models.Odoo/Accounting/AccountDocumentSyncFields.cs` | Lista única cabecera/detalle (HTTP + ZIP) |
| `ApiManagerOdoo/Accounting/HubAccountMove.cs` | `fields_array` → `AccountDocumentSyncFields.Header` |
| `ApiManagerOdoo/Accounting/HubAccountMoveLine.cs` | `fields_array` → `AccountDocumentSyncFields.Line` |
| `DMSA.Sync.Core/Update/AdminDocumentPackZipSync.cs` | Descarga / unzip / insert + validación de claves y mapeo |
| `DMSA.Sync.Core/Update/ServerPuller.cs` | Flag + `_adminDocumentPackImported` |
| `DMSA.Sync.Core/Update/ServerPuller.Accounting.cs` | Hook en cabecera y detalle |
| `DMCobranzas/AppPages/UpdateData.xaml.cs` | Comentario del flujo |
| `mnsa_mobile/models/mnsa_mobile_document_pack.py` | `HEADER_FIELDS` / `LINE_FIELDS` alineados |

**Odoo (ya existente, no es esta app):** cron diario 03:00, `date_to` = hoy, `date_from` = hace 1 año. Menú MINSA Mobile → Paquetes documentos.

**Un solo ZIP:** el cron borra paquetes y adjuntos anteriores (`_purge_all_document_packs`) y deja **un** registro `done` con su ZIP. Al generar manualmente también se purgan los demás. La tablet siempre toma el último `done` con adjunto.

**Probar:**

1. Usuario admin 218. Preferencia de “1ra del día” limpia (otro día, o `EnableForceFirstSyncOfDayUi = true` + check de prueba).
2. Cron o Generar ZIP en Odoo, estado `done` con adjunto.
3. Sync grupos 1+2: logs `[AdminDocumentPackZip]` importado; no páginas HTTP de account_move.
4. Sin pack: logs “no usado” y sigue HTTP.
5. Vendedor: no toca el ZIP.
6. Segundo sync el mismo día: HTTP incremental / rango UI.
7. ZIP viejo (sin los campos extra): log de claves faltantes y fallback HTTP.
8. Tras upgrade `mnsa_mobile` + regenerar ZIP: `docnum_mask` / `quantity_available` etc. llegan con valor.

---

*Informe generado — Cobranzas / Notas de crédito — Septiembre 2026*

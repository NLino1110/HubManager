# Informe: fallback de imágenes y pantalla de actualización responsive

Fecha: 2026-09-14  
Alcance: sync de catálogo de imágenes (`CatalogoImagenes`) y UI de **Actualización de datos** en DMOrders.

---

## 1. Resumen ejecutivo

| Problema | Causa | Solución |
|----------|--------|----------|
| Imágenes de `api.dmujeres.ec/images/` no se descargaban en sync | El código solo pedía la variante `{id}_1024.{ext}`, que en ese API devuelve **404** | Fallback: intentar `_1024` y, si falla, usar la URL original |
| En teléfono pequeño en horizontal no se podía bajar en **Actualización de datos** | Layout fijo con `VerticalStackLayout` sin scroll | `Grid` + `ScrollView` + botones en `FlexLayout` con wrap |

---

## 2. Contexto: descarga de imágenes por URL

### 2.1 Flujo actual de sync

1. `UpdateData.xaml.cs` dispara `serverPuller.OnlineSyncProductProductOnlyImagesUrl(...)`.
2. `ServerPuller.ProductProduct.cs` obtiene productos con `image_url` desde Odoo (`HubProductProduct.GetByWriteOnlyImageUrl`).
3. Descarga el binario vía `HttpClient`, lo guarda en SQLite (`product_product_preview`) como `image_1920` (base64) y genera `image_256` (resize a 256 px).
4. La UI (`ProductViewerRow.xaml.cs`) lee `image_1920` desde SQLite; no usa la URL en tiempo real.

### 2.2 Por qué existía `_1024`

Odoo maneja tiers de imagen (`image_256`, `image_512`, `image_1024`, `image_1920`). Al migrar de base64 embebido en RPC a URLs externas, se priorizó descargar una variante intermedia (~1024 px) transformando la URL:

```
https://cdn.ejemplo.com/36047.jpeg
→ https://cdn.ejemplo.com/36047_1024.jpeg
```

Objetivo: menos peso en sync que la original full, manteniendo calidad razonable.

### 2.3 Caso que fallaba

URL de ejemplo en Odoo:

```
https://api.dmujeres.ec/images/36047.jpeg
```

| URL solicitada | HTTP |
|----------------|------|
| `.../36047.jpeg` | **200 OK** (~69 KB) |
| `.../36047_1024.jpeg` | **404 Not Found** |

El API `api.dmujeres.ec/images/` sirve **un solo archivo por id**, sin sufijos de tamaño. El error se capturaba en silencio (`Debug.WriteLine`) y el producto quedaba sin imagen en SQLite → placeholder `image_not_found_gray_opt.png` en la app.

---

## 3. Cambio 1: fallback en descarga de imágenes

### 3.1 Archivo modificado

| Archivo | Rol |
|---------|-----|
| `DMSA.Sync.Core\Update\ServerPuller.ProductProduct.cs` | Sync de imágenes por URL |

### 3.2 Antes

```csharp
string sizeImg = "_1024";
var nameExt = Path.GetExtension(item.image_url);
var final_url = item.image_url.Replace(nameExt, sizeImg + nameExt);
var imageBytes = await httpClient.GetByteArrayAsync(final_url);
```

Solo se intentaba la variante `_1024`. Si no existía → excepción → sin imagen guardada.

### 3.3 Después

Nuevo helper `DownloadProductImageBytesAsync`:

1. Si la URL tiene extensión y **no** termina ya en `_1024`, intenta `{base}_1024.{ext}`.
2. Si esa petición lanza `HttpRequestException` (404, etc.), registra en Debug y **usa la URL original**.
3. Si la URL ya incluye `_1024`, descarga directo (evita `_1024_1024`).

El resto del proceso no cambia: base64 → `image_1920`, resize → `image_256`, `InsertBatchAsync`.

### 3.4 Comportamiento por origen

| Origen de `image_url` | Resultado |
|------------------------|-----------|
| `api.dmujeres.ec/images/36047.jpeg` | Falla `_1024` → descarga original ✅ |
| CDN con variante `_1024` | Sigue usando variante mediana ✅ |
| URL que ya es `..._1024.jpeg` | Descarga directa ✅ |

### 3.5 Documentación en código

Comentario XML en `DownloadProductImageBytesAsync` explicando el motivo del fallback y el caso `api.dmujeres.ec/images/`.

### 3.6 Cómo probar

1. Producto en Odoo con `image_url = https://api.dmujeres.ec/images/36047.jpeg`.
2. En DMOrders: **Actualización de datos** → marcar **Imágenes de productos** (o botón **Catálogo** parcial/total).
3. Verificar en catálogo que la imagen del producto se muestra (no el placeholder gris).
4. En Output/Debug, si aplica: `Imagen _1024 no disponible: ...` seguido de descarga exitosa con URL original.

---

## 4. Cambio 2: pantalla Actualización de datos responsive

### 4.1 Problema

En dispositivos móviles pequeños en **orientación horizontal**, el contenido (checkboxes + botones) excedía la altura visible. No había `ScrollView`, por lo que el usuario no podía desplazarse hacia abajo.

### 4.2 Archivo modificado

| Archivo | Rol |
|---------|-----|
| `DMOrders\Pages\Sys\UpdateData.xaml` | UI de selección de datos a sincronizar |

### 4.3 Antes

- Dos `VerticalStackLayout` anidados sin scroll.
- Botones en `HorizontalStackLayout` al final del stack (fuera del área visible en landscape bajo).

### 4.4 Después

Patrón alineado con `Connections.xaml`:

| Elemento | Función |
|----------|---------|
| `Grid RowDefinitions="*,Auto"` | Área principal flexible + franja fija inferior |
| `ScrollView` (fila `*`) | Checkboxes, estado Odoo y fechas scrollables |
| `FlexLayout Wrap="Wrap"` (fila `Auto`) | Botones siempre accesibles; si no caben en una fila, envuelven |
| `LineBreakMode="WordWrap"` | Título e info de sync no desbordan en pantallas estrechas |

### 4.5 Comportamiento esperado

| Orientación | Antes | Después |
|-------------|-------|---------|
| Vertical | OK | OK |
| Horizontal (teléfono pequeño) | Contenido cortado, sin scroll | Scroll en lista + botones visibles abajo |

### 4.6 Cómo probar

1. Abrir **Actualización de datos** en emulador o dispositivo físico.
2. Rotar a horizontal con pantalla pequeña (p. ej. 640×360 o similar).
3. Confirmar que se puede hacer scroll en la lista de checkboxes.
4. Confirmar que **Actualizar datos**, **Catálogo** y **Enviar datos** siguen visibles y pulsables (en una o dos filas según ancho).

---

## 5. Archivos tocados (resumen)

Rutas desde `HubManager\`.

| Archivo | Tipo de cambio |
|---------|----------------|
| `DMSA.Sync.Core\Update\ServerPuller.ProductProduct.cs` | Lógica: helper `DownloadProductImageBytesAsync` + documentación XML |
| `DMOrders\Pages\Sys\UpdateData.xaml` | UI: layout responsive con scroll y botones con wrap |

**Sin cambios** en code-behind (`UpdateData.xaml.cs`): mismos `x:Name` y event handlers.

---

## 6. Riesgos y consideraciones

| Tema | Nota |
|------|------|
| CDNs con `_1024` | Sin impacto: se sigue preferiendo la variante mediana |
| URLs sin extensión | No se aplica sufijo `_1024`; se descarga la URL tal cual |
| URLs con query string (`?v=...`) | `Path.GetExtension` puede no detectar extensión; comportamiento igual que antes |
| Rendimiento fallback | Un HTTP extra (404) solo cuando `_1024` no existe |
| Commit | Cambios locales; commit/push según flujo del equipo |

---

## 7. Referencias de código

- Entrada sync imágenes: `DMOrders\Pages\Sys\UpdateData.xaml.cs` → `OnlineSyncProductProductOnlyImagesUrl`
- Descarga + fallback: `DMSA.Sync.Core\Update\ServerPuller.ProductProduct.cs` → `DownloadProductImageBytesAsync`
- Consulta Odoo: `ApiManagerOdoo\HubProductProduct.cs` → `GetByWriteOnlyImageUrl`
- Visualización: `DMOrders\Controls\CustomRows\Lite\ProductViewerRow.xaml.cs` → `LoadImageAsync`

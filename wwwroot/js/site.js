// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Intercept cart form submissions
document.addEventListener('submit', async function(e) {
    const form = e.target;
    if (form.action && form.action.includes('/Carrito') && form.method === 'post') {
        e.preventDefault();
        const url = new URL(form.action);
        const id = url.searchParams.get('id');
        if (id) {
            await agregarAlCarrito(id);
        }
    }
});

function setNavbarTheme(theme) {
  const nav = document.querySelector(".navbar");
  if (!nav) return;

  if (theme === "dark") {
    nav.classList.remove("navbar-light", "bg-white");
    nav.classList.add("navbar-dark", "bg-dark");
  } else {
    nav.classList.remove("navbar-dark", "bg-dark");
    nav.classList.add("navbar-light", "bg-white");
  }
}

function setThemeIcon(theme) {
  const themeIcon = document.getElementById("themeIcon");
  if (!themeIcon) return;

  if (theme === "dark") {
    themeIcon.classList.remove("bi-sun-fill");
    themeIcon.classList.add("bi-moon-fill");
  } else {
    themeIcon.classList.remove("bi-moon-fill");
    themeIcon.classList.add("bi-sun-fill");
  }
}

function applyTheme(theme) {
  const htmlElement = document.documentElement;
  htmlElement.setAttribute("data-bs-theme", theme);
  localStorage.setItem("theme", theme);

  const themeSwitch = document.getElementById("themeSwitch");
  if (themeSwitch) {
    themeSwitch.checked = theme === "dark";
  }

  setNavbarTheme(theme);
  setThemeIcon(theme);
  htmlElement.classList.add("theme-transition");
}

function initThemeSwitcher() {
  const savedTheme = localStorage.getItem("theme") || "light";
  applyTheme(savedTheme);

  const themeSwitch = document.getElementById("themeSwitch");
  if (!themeSwitch) return;

  themeSwitch.addEventListener("change", () => {
    const newTheme = themeSwitch.checked ? "dark" : "light";
    applyTheme(newTheme);
  });
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initThemeSwitcher);
} else {
  initThemeSwitcher();
}

async function actualizarIconoCarrito() {
  try {
    const res = await fetch("/api/carrito/count");
    const text = await res.text();
    var count = text;
    if (count.startsWith('"') && count.endsWith('"')) {
      count = count.slice(1, -1);
    }
    const img = document.getElementById("carritoIcon");
    if (img) {
      img.src = parseInt(count) > 0
          ? "https://img.icons8.com/keek-line/48/shopping-cart-loaded.png"
          : "https://img.icons8.com/keek-line/48/shopping-cart.png";
    }
  } catch (e) {
    console.error('[Carrito] Error count:', e);
  }
}

async function agregarAlCarrito(productoId) {
    const formData = new FormData();
    formData.append('id', productoId);
    const res = await fetch('/api/carrito/agregar', { method: 'POST', body: formData });
    await actualizarIconoCarrito();
}

async function cargarCarrito() {
  const res = await fetch("/api/carrito/html");
  const html = await res.text();
  var clean = html;
  if (clean.startsWith('"') && clean.endsWith('"')) {
    clean = clean.slice(1, -1);
  }
  document.getElementById("carritoContenido").innerHTML = clean;
}

async function limpiarCarrito() {
  await fetch("/api/carrito/limpiar", { method: 'POST' });
  await cargarCarrito();
  await actualizarIconoCarrito();
}

async function cambiarCantidad(id, delta) {
  await fetch("/api/carrito/cambiar?id=" + id + "&delta=" + delta, { method: 'POST' });
  await cargarCarrito();
  await actualizarIconoCarrito();
}

async function eliminarProducto(id) {
  await fetch("/api/carrito/eliminar?id=" + id, { method: 'POST' });
  await cargarCarrito();
  await actualizarIconoCarrito();
}

document.addEventListener("DOMContentLoaded", function () {
  actualizarIconoCarrito();
  const carritoOffcanvas = document.getElementById("carritoOffcanvas");
  if (carritoOffcanvas) {
    carritoOffcanvas.addEventListener("show.bs.offcanvas", cargarCarrito);
  }
});

const editModal = document.getElementById("editModal");
if (editModal) {
  editModal.addEventListener("show.bs.modal", function (event) {
    const button = event.relatedTarget;
    document.getElementById("editId").value = button.getAttribute("data-id");
    document.getElementById("editNombre").value =
      button.getAttribute("data-name");
    document.getElementById("editPrecio").value =
      button.getAttribute("data-price");
    document.getElementById("editDescripcion").value =
      button.getAttribute("data-description");
    document.getElementById("editCategoria").value =
      button.getAttribute("data-category");
    document.getElementById("editImageUrl1").value =
      button.getAttribute("data-imageurl1") || "";
    document.getElementById("editImageUrl2").value =
      button.getAttribute("data-imageurl2") || "";
    document.getElementById("editImageUrl3").value =
      button.getAttribute("data-imageurl3") || "";
  });
}

const deleteModal = document.getElementById("deleteModal");
if (deleteModal) {
  deleteModal.addEventListener("show.bs.modal", function (event) {
    const button = event.relatedTarget;
    const id = button.getAttribute("data-id");
    const name = button.getAttribute("data-name");

    document.getElementById("productName").textContent = name;
    const form = document.getElementById("deleteForm");
    form.action = "?handler=Delete&id=" + id;
  });
}

    function toggleEdit() {
        const inputs = ['nombreInput', 'apellidoInput', 'emailInput', 'telefonoInput', 'fechaNacimientoInput'];
        const generoInput = document.getElementById('generoInput');
        const saveBtn = document.getElementById('saveButton');
        
        inputs.forEach(id => {
            const input = document.getElementById(id);
            if (input.readOnly) {
                input.removeAttribute('readonly');
                input.classList.add('bg-white', 'border', 'border-primary');
            } else {
                input.setAttribute('readonly', 'true');
                input.classList.remove('bg-white', 'border', 'border-primary');
            }
        });
        
        if (generoInput.disabled) {
            generoInput.removeAttribute('disabled');
            generoInput.classList.add('bg-white', 'border', 'border-primary');
        } else {
            generoInput.setAttribute('disabled', 'true');
            generoInput.classList.remove('bg-white', 'border', 'border-primary');
        }
        
        saveBtn.style.display = saveBtn.style.display === 'none' ? 'block' : 'none';
    }

    const perfilForm = document.getElementById('perfilForm');
if (perfilForm) {
    perfilForm.addEventListener('submit', function(e) {
        document.getElementById('hiddenNombre').value = document.getElementById('nombreInput').value;
        document.getElementById('hiddenApellido').value = document.getElementById('apellidoInput').value;
        document.getElementById('hiddenEmail').value = document.getElementById('emailInput').value;
        document.getElementById('hiddenTelefono').value = document.getElementById('telefonoInput').value;
        document.getElementById('hiddenGenero').value = document.getElementById('generoInput').value;
        document.getElementById('hiddenFechaNacimiento').value = document.getElementById('fechaNacimientoInput').value;
    });
}

const addDireccionBtn = document.getElementById('addDireccionBtn');
if (addDireccionBtn) {
    addDireccionBtn.addEventListener('click', function() {
        const form = document.getElementById('nuevaDireccionForm');
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    });
}

function agregarALista(productoId, event) {
    const btn = event.currentTarget;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value 
        || document.querySelector('form input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/Cuentas?handler=ObtenerListas', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token || ''
        },
        body: '__RequestVerificationToken=' + encodeURIComponent(token || '')
    })
    .then(res => {
        return res.text().then(text => {
            if (text.startsWith('<')) {
                showToast('Debes iniciar sesión para agregar a tu lista de deseos.', true, btn);
                return null;
            }
            try {
                return JSON.parse(text);
            } catch {
                showToast('Debes iniciar sesión para agregar a tu lista de deseos.', true, btn);
                return null;
            }
        });
    })
    .then(listas => {
        if (!listas) {
            return;
        }
        
        if (listas.success === false) {
            showToast('Debes iniciar sesión para agregar a tu lista de deseos.', true, btn);
            return;
        }
        
        fetch('/Productos?handler=ObtenerStock&id=' + productoId)
        .then(res => res.json())
        .then(data => {
            if (data.stock <= 0) {
                showToast('No hay existencias de este producto.', true, btn);
                return;
            }
            
            if (!listas || listas.length === 0) {
                showToast('No tienes listas creadas. Crea una primero en Mi Cuenta.', true, btn);
                return;
            }
            
            let popoverContent = '<div class="d-flex flex-column gap-2 p-2" style="min-width: 150px;">';
            listas.forEach(lista => {
                popoverContent += `<button class="btn btn-sm btn-outline-primary mb-1" onclick="agregarAListaEspecifica(${productoId}, ${lista.id}, this)">${lista.nombre}</button>`;
            });
            popoverContent += '</div>';
            
            if (btn._popover) {
                btn._popover.dispose();
            }
            
            btn.setAttribute('data-bs-toggle', 'popover');
            btn.setAttribute('data-bs-title', 'Seleccionar lista');
            
            btn._popover = new bootstrap.Popover(btn, {
                trigger: 'click',
                html: true,
                content: popoverContent,
                placement: 'bottom',
                sanitize: false
            });
            btn._popover.show();
        });
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Debes iniciar sesión para agregar a tu lista de deseos.', true, btn);
    });
}

function agregarAListaEspecifica(productoId, listaId, btn) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value 
        || document.querySelector('form input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/Cuentas?handler=AgregarAListaEspecifica', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token || ''
        },
        body: 'ProductoId=' + productoId + '&ListaId=' + listaId + '&__RequestVerificationToken=' + encodeURIComponent(token || '')
    })
    .then(response => response.json())
    .then(data => {
        const heartBtn = document.querySelector(`button[onclick*="agregarALista(${productoId}"]`);
        showToast(data.success ? 'Producto agregado a la lista' : (data.message || 'Error al agregar'), !data.success, heartBtn);
        if (heartBtn && heartBtn._popover) {
            heartBtn._popover.hide();
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Error al agregar a la lista', true);
    });
}

function showToast(message, isError = false, targetElement = null) {
    const toast = document.getElementById('liveToast');
    const toastBody = toast.querySelector('.toast-body');
    toastBody.textContent = message;
    toast.classList.remove('bg-success', 'bg-danger', 'text-white');
    toast.classList.add(isError ? 'bg-danger' : 'bg-success', 'text-white');
    
    if (targetElement) {
        const rect = targetElement.getBoundingClientRect();
        toast.style.position = 'fixed';
        toast.style.top = (rect.top + window.scrollY) + 'px';
        toast.style.left = (rect.right + 10 + window.scrollX) + 'px';
    } else {
        toast.style.position = '';
        toast.style.top = '';
        toast.style.left = '';
    }
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

function eliminarLista(listaId, btn) {
    if (!confirm('¿Estás seguro de que quieres eliminar esta lista?')) {
        return;
    }
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value 
        || document.querySelector('form input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/Cuentas?handler=EliminarLista', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token || ''
        },
        body: 'ListaId=' + listaId + '&__RequestVerificationToken=' + encodeURIComponent(token || '')
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('Lista eliminada', false);
            setTimeout(() => location.reload(), 1000);
        } else {
            showToast(data.message || 'Error al eliminar la lista', true);
        }
    })
    .catch(error => {
            showToast('Error al eliminar la lista', true);
    });
}

async function agregarListaAlCarrito(listaId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value 
        || document.querySelector('form input[name="__RequestVerificationToken"]')?.value;
    
    const response = await fetch('/Cuentas?handler=AgregarListaAlCarrito', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token || ''
        },
        body: 'ListaId=' + listaId + '&__RequestVerificationToken=' + encodeURIComponent(token || '')
    });
    
    const data = await response.json();
    
    if (data.success) {
        showToast('Productos agregados al carrito', false);
        actualizarIconoCarrito();
    } else {
        showToast(data.message || 'Error al agregar al carrito', true);
    }
}

async function agregarCarritoAjax(productoId) {
    actualizarIconoCarrito();
}

document.querySelectorAll('.descuento-checkbox').forEach(checkbox => {
    checkbox.addEventListener('change', function() {
        const id = this.getAttribute('data-id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        fetch('/Productos?handler=Descuento&id=' + id, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token || ''
            },
            body: '__RequestVerificationToken=' + encodeURIComponent(token || '')
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                this.checked = data.descuentoActivo;
                showToast(data.descuentoActivo ? 'Descuento activado' : 'Descuento desactivado', false);
            }
        });
    });
});
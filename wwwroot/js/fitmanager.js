/* ═══════════════════════════════════════════════════════════════════════════
   FitManager — Main JavaScript
   ═══════════════════════════════════════════════════════════════════════════ */

'use strict';

/* ── DataTables Spanish locale ──────────────────────────────────────────── */
var dtSpanish = {
  sEmptyTable:     'No hay datos disponibles',
  sInfo:           'Mostrando _START_ a _END_ de _TOTAL_ registros',
  sInfoEmpty:      'Mostrando 0 a 0 de 0 registros',
  sInfoFiltered:   '(filtrado de _MAX_ registros totales)',
  sLengthMenu:     'Mostrar _MENU_ registros',
  sLoadingRecords: 'Cargando...',
  sProcessing:     'Procesando...',
  sSearch:         '<i class="fas fa-search me-1"></i>',
  sSearchPlaceholder: 'Buscar...',
  sZeroRecords:    'No se encontraron resultados',
  oPaginate: {
    sFirst:    '<i class="fas fa-angle-double-left"></i>',
    sLast:     '<i class="fas fa-angle-double-right"></i>',
    sNext:     '<i class="fas fa-angle-right"></i>',
    sPrevious: '<i class="fas fa-angle-left"></i>'
  }
};

/* ── Sidebar toggle ─────────────────────────────────────────────────────── */
(function () {
  var COLLAPSED_KEY = 'fm-sidebar-collapsed';
  var body = document.body;

  // Restore collapsed state
  if (localStorage.getItem(COLLAPSED_KEY) === '1' && window.innerWidth >= 992) {
    body.classList.add('sidebar-collapsed');
  }

  // Desktop toggle (collapse/expand)
  var toggleBtn = document.getElementById('sidebarToggle');
  if (toggleBtn) {
    toggleBtn.addEventListener('click', function () {
      if (window.innerWidth >= 992) {
        body.classList.toggle('sidebar-collapsed');
        localStorage.setItem(COLLAPSED_KEY, body.classList.contains('sidebar-collapsed') ? '1' : '0');
      } else {
        body.classList.toggle('sidebar-open');
      }
    });
  }

  // Mobile: close button & overlay
  document.getElementById('sidebarClose')?.addEventListener('click', function () {
    body.classList.remove('sidebar-open');
  });
  document.getElementById('sidebarOverlay')?.addEventListener('click', function () {
    body.classList.remove('sidebar-open');
  });
})();

/* ── Auto-dismiss alerts ────────────────────────────────────────────────── */
(function () {
  document.querySelectorAll('.alert.alert-success, .alert.alert-danger').forEach(function (el) {
    setTimeout(function () {
      var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
      if (bsAlert) bsAlert.close();
    }, 5000);
  });
})();

/* ── Confirm delete (SweetAlert2) ───────────────────────────────────────── */
function confirmDeleteForms() {
  document.querySelectorAll('.confirm-delete-form').forEach(function (form) {
    if (form.dataset.confirmAttached) return;
    form.dataset.confirmAttached = '1';

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      Swal.fire({
        title: '¿Confirmar eliminación?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="fas fa-trash me-1"></i>Eliminar',
        cancelButtonText: 'Cancelar',
        background: '#1a1d27',
        color: '#e2e8f0',
        customClass: {
          popup: 'fm-swal-popup'
        }
      }).then(function (result) {
        if (result.isConfirmed) form.submit();
      });
    });
  });
}

/* ── Photo preview ──────────────────────────────────────────────────────── */
function previewPhoto(input, previewId) {
  var preview = document.getElementById(previewId);
  if (!preview || !input.files || !input.files[0]) return;

  var file = input.files[0];
  var maxSizeMB = 2;

  if (file.size > maxSizeMB * 1024 * 1024) {
    Swal.fire({
      title: 'Archivo muy grande',
      text: 'La imagen no puede superar ' + maxSizeMB + 'MB.',
      icon: 'error',
      background: '#1a1d27',
      color: '#e2e8f0'
    });
    input.value = '';
    return;
  }

  var reader = new FileReader();
  reader.onload = function (e) {
    preview.innerHTML = '<img src="' + e.target.result + '" style="width:100%;height:100%;object-fit:cover;" alt="Preview" />';
  };
  reader.readAsDataURL(file);
}

/* ── Run on DOM ready ───────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', function () {
  confirmDeleteForms();
});

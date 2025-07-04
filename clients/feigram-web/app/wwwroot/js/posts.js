function openUploadPictureModal() {
    var modal = new bootstrap.Modal(document.getElementById('uploadPictureModal'));
    modal.show();
}

function sendUploadedFormulary() {
    document.getElementById('uploadPictureForm').submit();
}

function previewImages(input) {
    const files = input.files;
    const container = document.getElementById('previewContainer');
    container.innerHTML = ''; // Limpia previos previews

    if (files.length > 0) {
        for (const file of files) {
            if (file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    img.className = 'img-fluid rounded shadow-sm border m-1';
                    img.style.maxHeight = '150px';
                    container.appendChild(img);
                };
                reader.readAsDataURL(file);
            }
        }

        container.style.display = 'flex';
        container.style.flexWrap = 'wrap';
        document.getElementById('textContainer').style.display = "block";
    } else {
        alert('Selecciona al menos una imagen.');
    }
}


function closeUploadPictureModal() {
    const modalElement = document.getElementById('uploadPictureModal');
    var modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) {
        modal.hide();
    }

    document.getElementById('inputImagen').value = '';
    document.getElementById('imagePreview').src = '';
    document.getElementById('previewContainer').style.display = 'none';
    document.getElementById('textContainer').style.display = 'none';

    const textarea = modalElement.querySelector('textarea[name="description"]');
    if (textarea) {
        textarea.value = '';
    }
}

document.getElementById('uploadPictureModal').addEventListener('hidden.bs.modal', function () {
    closeUploadPictureModal();
});

function openPostModal(postId, startIndex = 0) {
  console.log('openPostModal called with postId:', postId, 'startIndex:', startIndex);

  const jsonElement = document.getElementById(`post-data-${postId}`);
  if (!jsonElement) {
    console.error(`No se encontró el elemento post-data-${postId}`);
    return;
  }

  let post;
  try {
    post = JSON.parse(jsonElement.textContent);
    console.log('Post:', post);
  } catch (e) {
    console.error('Error parseando JSON:', e);
    return;
  }

  const container = document.getElementById("modalPostImageContainer");
  container.innerHTML = ""; // Limpiar contenido anterior

  if (post.imagenes.length > 1) {
    const carouselId = "modal-carousel";
    let indicators = "";
    let inner = "";

    for (let i = 0; i < post.imagenes.length; i++) {
      indicators += `
        <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="${i}"
          class="${i === startIndex ? 'active' : ''}"
          aria-current="${i === startIndex ? 'true' : 'false'}"
          aria-label="Slide ${i + 1}">
        </button>`;

      inner += `
        <div class="carousel-item ${i === startIndex ? 'active' : ''}">
          <img src="${post.imagenes[i]}" class="d-block w-100 rounded"
               style="max-height: 70vh; object-fit: contain;" />
        </div>`;
    }

    container.innerHTML = `
      <div id="${carouselId}" class="carousel slide" data-bs-ride="carousel">
        <div class="carousel-indicators">
          ${indicators}
        </div>
        <div class="carousel-inner">
          ${inner}
        </div>
        <button class="carousel-control-prev" type="button" data-bs-target="#${carouselId}" data-bs-slide="prev">
          <span class="carousel-control-prev-icon" aria-hidden="true"></span>
          <span class="visually-hidden">Anterior</span>
        </button>
        <button class="carousel-control-next" type="button" data-bs-target="#${carouselId}" data-bs-slide="next">
          <span class="carousel-control-next-icon" aria-hidden="true"></span>
          <span class="visually-hidden">Siguiente</span>
        </button>
      </div>
      <div class="mt-2"><span>${post.description}</span></div>
    `;
  } else if (post.imagenes.length === 1) {
    container.innerHTML = `
      <img src="${post.imagenes[0]}" class="img-fluid rounded"
           style="max-height: 70vh; object-fit: contain;" />
      <div class="mt-2"><span>${post.description}</span></div>
    `;
  }

  // Likes y PostId oculto
  document.getElementById('modalPostLikes').innerText = post.likes;
  document.getElementById('modalPostId').value = post.id;

  // Cargar comentarios
  fetch(`/Feed?handler=Comentarios&postId=${postId}`)
    .then(res => {
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return res.json();
    })
    .then(comments => {
      const commentsContainer = document.getElementById('modalPostComments');
      commentsContainer.innerHTML = "";

      comments.forEach(comment => {
        const p = document.createElement("p");
        p.innerText = `${comment.username}: ${comment.text}`;
        commentsContainer.appendChild(p);
      });
    })
    .catch(err => {
      console.error("❌ Error al obtener comentarios:", err);
    });

  const modal = new bootstrap.Modal(document.getElementById('consultPost'));
  modal.show();
}




fetch('/api/posts', {
    method: 'GET',
    credentials: 'include'
  })
  .then(response => response.json())
  .catch(err => console.error("Error al obtener posts:", err));

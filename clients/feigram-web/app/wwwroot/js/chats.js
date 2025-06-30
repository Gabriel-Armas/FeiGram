let socket = null;
let currentChatUserId = null;
let allContacts = [...contactProfiles]; // <-- Aquí guardaremos todos, inicializando con los que vinieron del backend

document.addEventListener("DOMContentLoaded", () => {
    console.log("[Client] DOM listo. Conectando WebSocket...");

    if (!jwtToken || jwtToken === "null") {
        console.error("[Client] ❌ No hay token disponible");
        return;
    }

    console.log("[Client] ✅ Ya conozco mi userId:", myUserId);
    connectWebSocket();
});

function connectWebSocket() {
    console.log(`[Client] 🔌 Conectando WebSocket con token (primeros 10): ${jwtToken.substring(0, 10)}...`);

    socket = new WebSocket(`wss://localhost/messages/ws/?token=${jwtToken}`);

    socket.onopen = () => {
        console.log("[Client] ✅ WebSocket conectado");
    
        requestContacts(); // 👈 lo mandas tú sin esperar al servidor
    };    

    socket.onmessage = (event) => {
        console.log("[Client] 📥 Mensaje recibido:", event.data);
        try {
            const data = JSON.parse(event.data);

            if (data.type === "user_id") {
                myUserId = data.userId;
                console.log("[Client] 🎯 Mi userId:", myUserId);
                requestContacts();
                return;
            }

            if (data.type === "contacts") {
                mergeAndRenderContacts(data.contacts);
                return;
            }

            if (data.type === "history") {
                renderChatHistory(data.messages);
            } else if (data.from) {
                handleIncomingMessage(data);
            }

        } catch (err) {
            console.error("[Client] ❌ Error parseando mensaje:", err);
        }
    };

    socket.onclose = (event) => {
        console.warn(`[Client] ❌ WebSocket cerrado. Código: ${event.code}`);
    };

    socket.onerror = (error) => {
        console.error("[Client] ❌ Error WebSocket:", error);
    };
}

function requestContacts() {
    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("[Client] 📨 Solicitando contactos WebSocket...");
        socket.send(JSON.stringify({
            type: "get_contacts"
        }));
    }
}

async function mergeAndRenderContacts(contactIds) {
    console.log("[Client] 🔄 Recibidos desde WebSocket:", contactIds);

    const newIds = contactIds.filter(id => !allContacts.some(c => c.id === id));

    for (const id of newIds) {
        try {
            const response = await fetch(`/profiles/profiles/${id}`, {
                headers: {
                    'Authorization': `Bearer ${jwtToken}`
                }
            });
            if (response.ok) {
                const profile = await response.json();
                allContacts.push(profile);
            } else {
                console.error(`[Client] ❌ No se encontró perfil para id=${id}`);
            }
        } catch (error) {
            console.error(`[Client] ❌ Error al obtener perfil para id=${id}`, error);
        }
    }

    console.log(`[Client] 👥 Total de contactos combinados: ${allContacts.length}`);
    renderContactList(allContacts);
}

function renderContactList(profiles) {
    const friendList = document.getElementById('friendList');
    friendList.innerHTML = '';

    profiles.forEach(profile => {
        const displayName = profile.username || profile.name; // Soporta ambos (por si viene de WebSocket)
        const photoUrl = profile.photo || '/images/default.png';
        const id = profile.id;

        const li = document.createElement('li');
        li.classList.add('list-group-item', 'list-group-item-action', 'd-flex', 'align-items-center');
        li.style.cursor = 'pointer';
        li.setAttribute('onclick', `openChat('${id}')`);

        li.innerHTML = `
            <img src="${photoUrl}" class="rounded-circle me-3" style="width: 40px; height: 40px; object-fit: cover;">
            <div>
                <strong>${displayName}</strong><br/>
                <small class="text-muted">Último mensaje...</small>
            </div>
        `;

        friendList.appendChild(li);
    });
}

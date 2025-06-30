let socket = null;
let currentChatUserId = null;
let allContacts = [...contactProfiles];

document.addEventListener("DOMContentLoaded", () => {
    console.log("[Client] DOM listo. Conectando WebSocket...");

    if (!jwtToken || jwtToken === "null") {
        console.error("[Client] No hay token disponible");
        return;
    }

    console.log("[Client] Mi userId:", myUserId);
    connectWebSocket();
});

function connectWebSocket() {
    console.log(`[Client] 🔌 Conectando WebSocket con token (primeros 10): ${jwtToken.substring(0, 10)}...`);

    socket = new WebSocket(`wss://localhost/messages/ws/?token=${jwtToken}`);

    socket.onopen = () => {
        console.log("[Client] WebSocket conectado");
    
        requestContacts();
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
            } else if (data.from_user || data.from) {
                const msg = {
                    from: data.from_user || data.from,
                    to: data.to,
                    content: data.content,
                    timestamp: data.timestamp
                };
            
                handleIncomingMessage(msg);
            }
            

        } catch (err) {
            console.error("[Client] Error parseando mensaje:", err);
        }
    };

    socket.onclose = (event) => {
        console.warn(`[Client] WebSocket cerrado. Código: ${event.code}`);
    };

    socket.onerror = (error) => {
        console.error("[Client] Error WebSocket:", error);
    };
}

function requestContacts() {
    if (socket && socket.readyState === WebSocket.OPEN) {
        console.log("[Client] Solicitando contactos WebSocket...");
        socket.send(JSON.stringify({
            type: "get_contacts"
        }));
    }
}

async function mergeAndRenderContacts(contactIds) {
    console.log("[Client] Recibidos desde WebSocket:", contactIds);

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
                console.error(`[Client] No se encontró perfil para id=${id}`);
            }
        } catch (error) {
            console.error(`[Client] Error al obtener perfil para id=${id}`, error);
        }
    }

    console.log(`[Client] 👥 Total de contactos combinados: ${allContacts.length}`);
    renderContactList(allContacts);
}

function renderContactList(profiles) {
    const friendList = document.getElementById('friendList');
    friendList.innerHTML = '';

    profiles.forEach(profile => {
        const displayName = profile.username || profile.name;
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
                <small class="text-muted"></small>
            </div>
        `;

        friendList.appendChild(li);
    });
}

function openChat(userId) {
    currentChatUserId = userId;

    const profile = allContacts.find(p => p.id === userId);
    if (profile) {
        document.getElementById("chatTitle").innerText = profile.username || profile.name;
    }

    document.getElementById("chatMessages").innerHTML = '';

    if (socket && socket.readyState === WebSocket.OPEN) {
        socket.send(JSON.stringify({
            type: "start_chat",
            with: userId
        }));
    }
}

function renderChatHistory(messages) {
    const chatContainer = document.getElementById("chatMessages");
    chatContainer.innerHTML = '';

    messages.forEach(msg => appendMessage(msg));
}

function handleIncomingMessage(msg) {
    if (msg.from === currentChatUserId || msg.to === currentChatUserId) {
        appendMessage(msg);
    }
}

function appendMessage(msg) {
    const isMine = msg.from === myUserId;
    const chatContainer = document.getElementById("chatMessages");

    const row = document.createElement("div");
    row.className = `d-flex mb-2 ${isMine ? 'justify-content-end' : 'justify-content-start'}`;

    const bubble = document.createElement("div");
    bubble.className = `p-2 rounded ${isMine ? 'bg-primary text-white' : 'bg-light text-dark'}`;
    bubble.style.maxWidth = "70%";
    bubble.style.wordBreak = "break-word";
    bubble.textContent = msg.content;

    row.appendChild(bubble);
    chatContainer.appendChild(row);

    chatContainer.scrollTop = chatContainer.scrollHeight;
}

function sendMessage() {
    const input = document.getElementById("chatInput");
    const content = input.value.trim();
    if (!content || !currentChatUserId) return;

    const message = {
        to: currentChatUserId,
        content: content
    };

    socket.send(JSON.stringify(message));

    appendMessage({
        from: myUserId,
        to: currentChatUserId,
        content: content
    });

    input.value = '';
}
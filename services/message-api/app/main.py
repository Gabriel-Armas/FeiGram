from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from app.websocket import handle_messages
from app.auth import get_current_user

app = FastAPI()

"""
@app.websocket("/ws/")
async def websocket_endpoint(websocket: WebSocket, token: str):
    await websocket.accept()

    try:
        user_id = await get_current_user(token)
    except Exception:
        await websocket.close(code=1008)
        return

    await handle_messages(user_id, websocket)
"""

@app.websocket("/ws/")
async def websocket_endpoint(websocket: WebSocket, token: str):
    await websocket.accept()

    user_id = token

    await handle_messages(user_id, websocket)

from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from app.websocket import handle_messages
from app.auth import get_current_user, JWTError
from jose.exceptions import ExpiredSignatureError

app = FastAPI()

@app.websocket("/ws/")
async def websocket_endpoint(websocket: WebSocket, token: str):
    await websocket.accept()

    try:
        user_id = await get_current_user(token)
    except ExpiredSignatureError:
        await websocket.close(code=4003) 
        return
    except JWTError:
        await websocket.close(code=4001) 
        return
    except Exception:
        await websocket.close(code=4000) 
        return

    await handle_messages(user_id, websocket)

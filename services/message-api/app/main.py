from fastapi import FastAPI, WebSocket, status
from app.websocket import handle_messages
from app.auth import get_current_user, JWTError
from jose.exceptions import ExpiredSignatureError
import logging

logger = logging.getLogger("websocket")
app = FastAPI()

@app.websocket("/ws/")
async def websocket_token_query(websocket: WebSocket, token: str):
    await websocket.accept()
    try:
        user_id = await get_current_user(token)
    except ExpiredSignatureError:
        logger.warning("Token expirado en /ws/")
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return
    except JWTError:
        logger.warning("Token inválido en /ws/")
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return
    except Exception as e:
        logger.error(f"Error interno en /ws/: {e}")
        await websocket.close(code=status.WS_1011_INTERNAL_ERROR)
        return

    await handle_messages(user_id, websocket)


@app.websocket("/message/")
async def websocket_cookie(websocket: WebSocket):
    await websocket.accept()

    token = websocket.cookies.get("jwt_token")
    if not token:
        logger.warning("No se encontró cookie jwt_token en /message/")
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return

    try:
        user_id = await get_current_user(token)
    except ExpiredSignatureError:
        logger.warning("Token expirado en /message/")
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return
    except JWTError:
        logger.warning("Token inválido en /message/")
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return
    except Exception as e:
        logger.error(f"Error interno en /message/: {e}")
        await websocket.close(code=status.WS_1011_INTERNAL_ERROR)
        return

    await handle_messages(user_id, websocket)

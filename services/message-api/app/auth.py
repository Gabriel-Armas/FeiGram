import logging
import os
from jose import jwt, JWTError, ExpiredSignatureError
from fastapi import WebSocketException, status
from dotenv import load_dotenv
import os

load_dotenv()

JWT_SECRET_KEY = os.getenv("JWT_SECRET_KEY")
JWT_ISSUER = os.getenv("JWT_ISSUER")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE")
JWT_ALGORITHM = "HS256"

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

async def get_current_user(token: str) -> str:
    try:
        payload = jwt.decode(
            token,
            JWT_SECRET_KEY,
            algorithms=[JWT_ALGORITHM],
            issuer=JWT_ISSUER,
            audience=JWT_AUDIENCE
        )

        role = payload.get("role")
        if role == "Banned":
            logger.warning("Intento de conexión con rol Banned")
            raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)

        user_id = payload.get("sub")
        if not user_id:
            logger.error("Token recibido sin 'sub'")
            raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)

        return user_id

    except ExpiredSignatureError as e:
        logger.error(f"Token expirado: {e}")
        raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)

    except JWTError as e:
        logger.error(f"Error JWT: {e}")
        raise WebSocketException(code=status.WS_1008_POLICY_VIOLATION)
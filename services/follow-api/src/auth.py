from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from jose import jwt, JWTError, ExpiredSignatureError
import os
from dotenv import load_dotenv

load_dotenv()

JWT_SECRET_KEY = os.getenv("JWT_SECRET_KEY")
JWT_ISSUER = os.getenv("JWT_ISSUER")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE")
JWT_ALGORITHM = "HS256"

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")

async def get_current_user(token: str = Depends(oauth2_scheme)) -> str:
    try:
        payload = jwt.decode(
            token,
            JWT_SECRET_KEY,
            algorithms=[JWT_ALGORITHM],
            issuer=JWT_ISSUER,
            audience=JWT_AUDIENCE,
        )

        role = payload.get("role")
        if role == "Banned":
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="Usuario baneado"
            )

        user_id = payload.get("sub")
        if not user_id:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token inválido: falta 'sub'"
            )

        return user_id

    except ExpiredSignatureError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token expirado"
        )

    except JWTError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token inválido"
        )
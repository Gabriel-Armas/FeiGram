from jose import JWTError, jwt
from fastapi import HTTPException, Depends
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import os
from dotenv import load_dotenv

load_dotenv("./env/.env")

JWT_SECRET_KEY = os.getenv("JWT_SECRET_KEY")
JWT_ISSUER = os.getenv("JWT_ISSUER")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE")

security = HTTPBearer()

async def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    try:
        payload = jwt.decode(
            token,
            JWT_SECRET_KEY,
            audience=JWT_AUDIENCE,
            issuer=JWT_ISSUER,
            algorithms=["HS256"]
        )
        return payload
    except JWTError:
        raise HTTPException(status_code=403, detail="Invalid token")

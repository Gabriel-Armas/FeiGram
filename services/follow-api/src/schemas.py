from pydantic import BaseModel

class UserCreate(BaseModel):
    id: str

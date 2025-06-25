from pydantic import BaseModel
from typing import Optional
from datetime import datetime

class PostCreate(BaseModel):
    id_usuario: Optional[int] = None
    descripcion: str
    url_media: str
    fechaPublicacion: datetime

class PostResponse(PostCreate):
    post_id: int
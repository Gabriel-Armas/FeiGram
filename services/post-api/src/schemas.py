from pydantic import BaseModel, Field
from typing import Optional
from datetime import datetime
import pytz

def get_local_datetime():
    # Zona horaria de Veracruz / Ciudad de México
    tz = pytz.timezone("America/Mexico_City")
    return datetime.now(tz)

class PostCreate(BaseModel):
    id_usuario: Optional[int] = None
    descripcion: str
    url_media: str
    fechaPublicacion: datetime = Field(default_factory=get_local_datetime)

class PostResponse(PostCreate):
    post_id: int
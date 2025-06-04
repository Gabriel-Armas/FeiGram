from fastapi import APIRouter, File, UploadFile, HTTPException
import cloudinary.uploader
import src.cloudinary_config

router = APIRouter()

@router.post("/upload-image")
async def upload_image(file: UploadFile = File(...)):
    try:
        result = cloudinary.uploader.upload(file.file)
        return {
            "url": result["secure_url"],
            "public_id": result["public_id"]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

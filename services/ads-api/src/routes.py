from fastapi import APIRouter, File, Form, UploadFile, HTTPException, Depends, Path
from database import ads_collection
from cloudinary.uploader import upload
from cloudinary.exceptions import Error as CloudinaryError
from auth import get_current_user
from datetime import datetime
from bson import ObjectId
import random

router = APIRouter()

@router.get("/ads", status_code=200)
def get_all_ads(current_user: dict = Depends(get_current_user)):
    if current_user.get("role") != "Admin":
        raise HTTPException(status_code=403, detail="Acceso denegado. Solo administradores pueden ver los anuncios.")
    try:
        ads_cursor = ads_collection.find()
        ads = []
        for ad in ads_cursor:
            ads.append({
                "id": str(ad.get("_id")),
                "brandName": ad.get("brandName"),
                "publicationDate": ad.get("publicationDate"),
                "urlMedia": ad.get("urlMedia"),
                "urlSite": ad.get("urlSite"),
                "description": ad.get("description"),
                "amount": ad.get("amount", 1)
            })
        return {"ads": ads}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error al obtener los anuncios: {str(e)}")

@router.post("/ads", status_code=201)
def create_ad(
    brandName: str = Form(...),
    urlSite: str = Form(...),
    description: str = Form(...),
    amount: int = Form(...),
    file: UploadFile = File(...),
    current_user: dict = Depends(get_current_user)
):
    if current_user.get("role") != "Admin":
        raise HTTPException(status_code=403, detail="Acceso denegado. Solo administradores pueden publicar anuncios.")

    try:
        upload_result = upload(file.file)
        image_url = upload_result.get("secure_url")
        if not image_url:
            raise HTTPException(status_code=500, detail="Fallo al obtener la URL del archivo en Cloudinary.")

        ad_data = {
            "brandName": brandName,
            "publicationDate": datetime.utcnow().isoformat(),
            "urlMedia": image_url,
            "urlSite": urlSite,
            "description": description,
            "amount": amount
        }

        result = ads_collection.insert_one(ad_data)

        return {
            "message": "Anuncio creado exitosamente.",
            "ad_id": str(result.inserted_id)
        }

    except CloudinaryError as ce:
        raise HTTPException(status_code=500, detail=f"Error al subir el archivo a Cloudinary: {str(ce)}")

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error inesperado al crear el anuncio: {str(e)}")

@router.get("/ads/priority", status_code=200)
def get_random_ad(current_user: dict = Depends(get_current_user)):
    if current_user.get("role") == "Banned":
        raise HTTPException(status_code=403, detail="Usuario baneado. No puede ver anuncios.")

    try:
        ads = list(ads_collection.find())
        if not ads:
            raise HTTPException(status_code=404, detail="No hay anuncios disponibles.")

        weights = [ad.get("amount", 1) for ad in ads]
        selected = random.choices(ads, weights=weights, k=1)[0]

        return {
            "id": str(selected.get("_id")),
            "brandName": selected.get("brandName"),
            "publicationDate": selected.get("publicationDate"),
            "urlMedia": selected.get("urlMedia"),
            "urlSite": selected.get("urlSite"),
            "description": selected.get("description"),
            "amount": selected.get("amount", 1)
        }

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error al seleccionar el anuncio: {str(e)}")

@router.delete("/ads/{ad_id}", status_code=200)
def delete_ad(ad_id: str = Path(..., description="ID del anuncio a eliminar"), current_user: dict = Depends(get_current_user)):
    if current_user.get("role") != "Admin":
        raise HTTPException(status_code=403, detail="Acceso denegado. Solo administradores pueden eliminar anuncios.")
    
    if not ObjectId.is_valid(ad_id):
        raise HTTPException(status_code=400, detail="ID de anuncio inválido.")
    
    result = ads_collection.delete_one({"_id": ObjectId(ad_id)})
    
    if result.deleted_count == 0:
        raise HTTPException(status_code=404, detail="Anuncio no encontrado.")
    
    return {"message": f"Anuncio con id {ad_id} eliminado exitosamente."}

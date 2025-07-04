import cloudinary
import cloudinary.uploader
import os
from dotenv import load_dotenv

load_dotenv("./env/.env")

cloudinary.config(
    cloud_name=os.getenv("CLOUDINARY_CLOUD_NAME"),
    api_key=os.getenv("CLOUDINARY_API_KEY"),
    api_secret=os.getenv("CLOUDINARY_API_SECRET")
)

def upload_image(file):
    upload_result = cloudinary.uploader.upload(file.file)
    return upload_result["secure_url"]
package com.example.feigram.network.model.posts

import com.google.gson.annotations.SerializedName

data class PostCreateRequest(
    @SerializedName("descripcion") val description: String,
    @SerializedName("url_media") val urlMedia: String,
    @SerializedName("fechaPublicacion") val publicationDate: String
)
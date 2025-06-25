package com.example.feigram.network.model.posts

import com.google.gson.annotations.SerializedName

data class PostResponse(
    @SerializedName("post_id")
    val postId: Int,

    @SerializedName("id_usuario")
    val userId: String,

    @SerializedName("descripcion")
    val description: String,

    @SerializedName("url_media")
    val urlMedia: String,

    @SerializedName("fechaPublicacion")
    val publicationDate: String
)

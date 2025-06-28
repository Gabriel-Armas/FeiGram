package com.example.feigram.network.model.posts

import com.google.gson.annotations.SerializedName

data class Comment(
    @SerializedName("post_id")
    val postId: String,

    @SerializedName("user_id")
    val userId: String,

    @SerializedName("text_comment")
    val textComment: String,

    @SerializedName("created_at")
    val createdAt: String
)

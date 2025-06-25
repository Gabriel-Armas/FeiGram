package com.example.feigram.network.model.posts

data class LikeCountResponse(
    val postId: String,
    val likeCount: Int
)
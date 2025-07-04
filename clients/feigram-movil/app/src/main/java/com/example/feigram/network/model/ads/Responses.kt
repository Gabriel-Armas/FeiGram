package com.example.feigram.network.model.ads

data class CreateAdResponse(
    val message: String,
    val ad_id: String
)

data class DeleteAdResponse(
    val message: String
)

data class AdsResponse(
    val ads: List<Ad>
)

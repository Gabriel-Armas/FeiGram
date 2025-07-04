package com.example.feigram.network.api

import com.example.feigram.network.model.ads.Ad
import com.example.feigram.network.model.ads.AdsResponse
import com.example.feigram.network.model.ads.CreateAdResponse
import com.example.feigram.network.model.ads.DeleteAdResponse
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.Response
import retrofit2.http.*

interface AdsApi {

    @GET("ads/ads")
    suspend fun getAllAds(
        @Header("Authorization") token: String
    ): Response<AdsResponse>

    @Multipart
    @POST("ads/ads")
    suspend fun createAd(
        @Header("Authorization") token: String,
        @Part("brandName") brandName: RequestBody,
        @Part("urlSite") urlSite: RequestBody,
        @Part("description") description: RequestBody,
        @Part("amount") amount: RequestBody,
        @Part file: MultipartBody.Part
    ): Response<CreateAdResponse>

    @GET("ads/ads/priority")
    suspend fun getRandomAd(
        @Header("Authorization") token: String
    ): Response<Ad>

    @DELETE("ads/ads/{ad_id}")
    suspend fun deleteAd(
        @Header("Authorization") token: String,
        @Path("ad_id") adId: String
    ): Response<DeleteAdResponse>
}

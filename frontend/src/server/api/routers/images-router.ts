import { z } from "zod";
import { createTRPCRouter, publicProcedure } from "~/server/api/trpc";
import { faker } from '@faker-js/faker';
import { ImageFile, Pagination } from "~/models/image-file.model";
import sharp from 'sharp';
import fetch from 'node-fetch';
import { env } from "~/env.mjs";
import { Agent } from 'https';
import { SignedFileUrl } from "~/models/signed-file-url.model";
import { ImageFileWithSignedUrl } from "~/models/image-file-with-signed-url";

const agent = new Agent({
    rejectUnauthorized: false
});
export const imagesRouter = createTRPCRouter({
    fetch: publicProcedure.input(z.object({
        page: z.number().default(1),
        limit: z.number().default(10),
        sortBy: z.string().default('Name'),
        order: z.string().default('asc'),
    }))
        .query(async ({ input }) => {
            const { page, limit, sortBy, order } = input;
            const result = await fetch(`${env.API_URL}ImageFiles?page=${page}&limit=${limit}&sortBy=${sortBy}&order=${order}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'accept': 'application/json',
                },
                agent
            })
            const response = await result.json();
            return response as Pagination<ImageFileWithSignedUrl>;
        }),

    getPresignedUploadURLs: publicProcedure.input(z.object({
        fileNames: z.array(z.string()),
    })).mutation(async ({ input }) => {
        const { fileNames } = input;
        const body = JSON.stringify({ fileNames });

        const result = await fetch(`${env.API_URL}AWSS3/presignedUrls`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'accept': 'application/json',
            },
            body: body,
            agent
        })

        const data = await result.json() as SignedFileUrl[];
        return data;

    }),

    postUploadComplete: publicProcedure.input(z.object({
        fileUploadedItems: z.array(z.object({
            FileName: z.string(),
            S3Location: z.string()
        }))
    })).mutation(async ({ input }) => {
        const { fileUploadedItems } = input;

        const body = JSON.stringify({ fileUploadedItems });

        const result = await fetch(`${env.API_URL}ImageFiles/postUpload`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'accept': 'application/json',
            },
            body: body,
            agent
        })

        const data = await result.json() as { message: string, isSuccessfully: boolean };
        return data.isSuccessfully;

    }),
    delete: publicProcedure.input(z.object({
        id: z.string(),
    })).mutation(async ({ input }) => {
        const { id } = input;

        const result = await fetch(`${env.API_URL}ImageFiles/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'accept': 'application/json',
            },
            agent
        })

        const data = await result.json() as { message: string };
        return data.message;
    }),
    update: publicProcedure.input(z.object({
        id: z.string(),
        description: z.string(),
    })).mutation(async ({ input }) => {
        const { id, description } = input;

        const body = JSON.stringify({ description });

        const result = await fetch(`${env.API_URL}ImageFiles/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'accept': 'application/json',
            },
            body: body,
            agent
        })

        const data = await result.json() as { message: string };
        return data.message;
    }),

});

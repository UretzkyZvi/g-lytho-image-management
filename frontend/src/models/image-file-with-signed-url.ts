import { ImageFile } from "./image-file.model";

export interface ImageFileWithSignedUrl {
    imageFile: ImageFile;
    signedUrl: string;
}
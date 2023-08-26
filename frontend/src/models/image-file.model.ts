export interface ImageFile {
    id: string;
    name: string;
    url: string;
    size: number;
    createdAt: string;
    updatedAt: string;
    uploadedBy: string;
    metadata?: any;
    description?: string;
}

export interface Pagination<T> {
    data: T[];
    total: number;
    totalPage: number;
    page: number;
    limit: number;
    nextPageLink?: string;
}
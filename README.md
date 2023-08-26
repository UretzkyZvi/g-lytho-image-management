# Image Management Web App

Welcome to the technical assessment project for the full-stack developer position at Lytho. This web application allows users to upload images, view them in a paginated 3-column grid format, and see an enlarged version of the image with a properties pane for metadata. 

## Table of Contents

- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Technologies Used](#technologies-used)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Features

- **Image Upload**: Users can upload their desired images.
- **Image Gallery**: Images are displayed in a paginated 3-column grid format.
- **Image Metadata**: Clicking on an image reveals a properties pane showing the image's metadata.
- **CRUD Functionality**: The application supports uploading, viewing, updating, and deleting images.

## API Endpoints

1. **AWSS3 Pre-signed URLs**
   - **Endpoint**: `/AWSS3/presignedUrls`
   - **Method**: `POST`
   - **Description**: Get pre-signed URLs for image uploads.
   
2. **Image Retrieval**
   - **Endpoint**: `/ImageFiles`
   - **Method**: `GET`
   - **Description**: Retrieve images with pagination, sorting, and ordering options.
   
3. **Image Post-Upload**
   - **Endpoint**: `/ImageFiles/postUpload`
   - **Method**: `POST`
   - **Description**: Handle post-upload procedures for images.
   
4. **Image Update**
   - **Endpoint**: `/ImageFiles/{id}`
   - **Method**: `PUT`
   - **Description**: Update the description of an uploaded image.
   
5. **Image Deletion**
   - **Endpoint**: `/ImageFiles/{id}`
   - **Method**: `DELETE`
   - **Description**: Delete an uploaded image.

## Technologies Used

- **Backend**: .NET Core Web API
- **Database**: MongoDB Cloud
- **Frontend**: Next.js hosted on Vercel
- **Storage**: AWS S3
- **Deployment**: AWS Elastic Beanstalk
- **Secret Management**: AWS Secret Manager
- **SSL**: Self-signed certificate for AWS Load Balancer

## License

This project is licensed under the MIT License.

## Acknowledgements

- Thanks to Lytho for providing the opportunity and guidelines for this assessment.
 
 

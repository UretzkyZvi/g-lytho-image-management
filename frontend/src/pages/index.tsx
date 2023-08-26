import { fa } from "@faker-js/faker";
import Head from "next/head";
import Link from "next/link";
import { use, useCallback, useEffect, useRef, useState } from "react";
import { Filters } from "~/components/filters";
import { ImageDetailsSidebar } from "~/components/image-details.sidebar";
import { ImageFileWithSignedUrl } from "~/models/image-file-with-signed-url";

import { api } from "~/utils/api";

export default function Home() {
  const [images, setImages] = useState<ImageFileWithSignedUrl[]>([]);
  const fetchingRef = useRef(false);
  const [ulHeight, setUlHeight] = useState("auto");
  const titleRef = useRef<HTMLDivElement>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  // side-bar functionality
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [selectedImage, setSelectedImage] = useState<ImageFileWithSignedUrl>();

  const [filters, setFilters] = useState({
    name: "",
    size: "",
    description: "",
  });

  const files = api.images.fetch.useQuery(
    { page, limit: 2, ...filters },
    {
      enabled: hasMore,
    }
  );

  useEffect(() => {
    setPage(1);
  }, [filters]);

  useEffect(() => {
    if (files.data !== undefined) {
      setImages((prev) => {
        const newImages = files.data.data;
        const existingImageIds = new Set(prev.map((img) => img.imageFile.id));
        const uniqueNewImages = newImages.filter(
          (img) => !existingImageIds.has(img.imageFile.id)
        );
        return [...prev, ...uniqueNewImages];
      });
      setHasMore(files.data.nextPageLink !== null);
      fetchingRef.current = false;
    }
    setLoading(false);
  }, [files.data]);

  useEffect(() => {
    const delayCheck = () => {
      const ulElement = document.querySelector("ul");
      if (
        ulElement &&
        ulElement.scrollHeight <= ulElement.clientHeight &&
        hasMore &&
        !fetchingRef.current
      ) {
        setPage((prev) => prev + 1);
        fetchingRef.current = true;
      }
    };
    const timeoutId = setTimeout(delayCheck, 1000); // 200ms delay

    return () => clearTimeout(timeoutId); // Clear the timeout if the component is unmounted
  }, [images, hasMore]);

  useEffect(() => {
    setImages([]); // Clear the images list
    setPage(1); // Reset the page to 1
  }, []);

  useEffect(() => {
    const calculateHeight = () => {
      if (titleRef.current) {
        // Get the title height and any other space above the ul
        const titleHeight = titleRef.current.offsetHeight;
        const mainPadding = 64; // gap-12 + m-2*2 =  48px + 16px= 64px
        const otherSpace = 0; // Add any other space if necessary

        // Calculate the available height for ul
        const height =
          window.innerHeight - titleHeight - mainPadding - otherSpace;

        setUlHeight(`${height}px`);
      }
    };

    // Call once to set the initial height
    calculateHeight();

    // Update height on window resize
    window.addEventListener("resize", calculateHeight);

    // Cleanup
    return () => {
      window.removeEventListener("resize", calculateHeight);
    };
  }, []);

  const handleScroll = (e: React.UIEvent<HTMLUListElement>) => {
    if (loading || !hasMore || fetchingRef.current) return; // Check fetchingRef here

    const ulElement = e.currentTarget;
    if (
      ulElement.scrollHeight - ulElement.scrollTop <=
      ulElement.clientHeight + 100
    ) {
      setLoading(true);
      setPage((prev) => prev + 1);
      fetchingRef.current = true; // Set fetchingRef to true here
    }
  };

  const handleImageClick = useCallback(
    (image: ImageFileWithSignedUrl) => {
      setSelectedImage(image);
      setSidebarOpen(true);
    },
    [setSelectedImage, setSidebarOpen]
  );
  const deleteMutation = api.images.delete.useMutation();
  const updateMutation = api.images.update.useMutation();
  const handleOnDeleteImage = useCallback(
    (imageId: string) => {
      setImages((prev) => prev.filter((img) => img.imageFile.id !== imageId));
      deleteMutation.mutateAsync({
        id: imageId,
      });
    },
    [setImages]
  );

  const handleOnUpdateImage = useCallback(
    (imageId: string, description: string) => {
      const prevDescription = selectedImage?.imageFile.description;
      setSelectedImage((prev) => {
        if (prev) {
          return {
            ...prev,
            imageFile: {
              ...prev.imageFile,
              description: description,
            },
          };
        }
        return prev;
      });

      updateMutation
        .mutateAsync({
          id: imageId,
          description,
        })
        .then(() => {
          console.log("success");
        })
        .catch(() => {
          // On error, revert description and show error message
          setSelectedImage((prev) => {
            if (prev) {
              return {
                ...prev,
                imageFile: {
                  ...prev.imageFile,
                  description: prevDescription,
                },
              };
            }
            return prev;
          });
        });
    },
    [setImages, selectedImage]
  );

  return (
    <>
      <Head>
        <title>G-Lytho</title>
        <meta name="description" content="Generated by Greg Uretzky" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main className="flex min-h-screen flex-col items-center bg-gray-200  tracking-tight">
        <div className="container">
          <div className="mx-2 flex flex-col gap-12  sm:mx-0">
            <div
              ref={titleRef}
              className="flex items-center justify-center text-white "
            >
              <h2 className="text-5xl font-extrabold  text-black sm:text-[5rem]">
                Upload <span className="text-fuchsia-700">&</span> Zoom
              </h2>
            </div>
            <div className=" flex flex-row items-center justify-between">
              <Link
                href="/upload-images"
                className="flex items-center justify-center rounded-md border border-transparent bg-fuchsia-600 px-4 py-2 text-base font-medium text-white shadow-sm hover:bg-fuchsia-700"
              >
                Upload Images
              </Link>
            </div>

            <div className="flex flex-col items-center justify-center gap-12 overflow-y-auto ">
              <ul
                role="list"
                className="m-2 grid min-h-[550px] grid-cols-2 gap-x-4 gap-y-8 overflow-y-auto sm:grid-cols-3 sm:gap-x-6 lg:grid-cols-3 xl:gap-x-8"
                style={{ maxHeight: ulHeight }}
                onScroll={handleScroll}
              >
                {images &&
                  images.map((item, i) => (
                    <li key={`${item.imageFile.id}_${i}`} className="relative">
                      <div className="aspect-h-7 aspect-w-10 group  flex min-h-[400px] w-full items-center justify-center overflow-hidden rounded-lg bg-black focus-within:ring-2 focus-within:ring-indigo-500 focus-within:ring-offset-2 focus-within:ring-offset-gray-100">
                        <img
                          src={item.signedUrl}
                          alt=""
                          className="pointer-events-none max-h-80 w-96 object-cover group-hover:opacity-75"
                        />
                      </div>

                      <button
                        type="button"
                        className="absolute inset-0 focus:outline-none"
                        onClick={() => {
                          handleImageClick(item);
                        }}
                      >
                        <span className="sr-only">
                          View details for {item.imageFile.name}
                        </span>
                      </button>

                      <p className="pointer-events-none mt-2 block truncate text-sm font-medium text-gray-900">
                        {item.imageFile.name}
                      </p>
                      <p className="pointer-events-none block text-sm font-medium text-gray-500">
                        {item.imageFile.size.toPrecision(2)} MB
                      </p>
                    </li>
                  ))}
              </ul>
            </div>
          </div>
        </div>
        {selectedImage && (
          <ImageDetailsSidebar
            open={sidebarOpen}
            setOpen={setSidebarOpen}
            selectedImage={selectedImage}
            onDeleteImage={handleOnDeleteImage}
            updateImageDescription={handleOnUpdateImage}
          />
        )}
      </main>
    </>
  );
}

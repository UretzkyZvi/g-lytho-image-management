import { FC, use, useCallback, useEffect } from "react";
import { Fragment, useState } from "react";
import { Dialog, Transition } from "@headlessui/react";
import { X, Heart, Pen, PlusCircle } from "lucide-react";
import { ImageFile } from "~/models/image-file.model";
import { DateTime } from "luxon";
import { ImageZoomModal } from "./image-zoom.modal";
import { set } from "zod";
import { ImageFileWithSignedUrl } from "~/models/image-file-with-signed-url";
import { on } from "events";

interface ImageDetailsSidebarProps {
  open: boolean;
  setOpen: (open: boolean) => void;
  selectedImage: ImageFileWithSignedUrl;
  updateImageDescription: (imageId: string, description: string) => void;
  onDeleteImage: (imageId: string) => void;
}

export const ImageDetailsSidebar: FC<ImageDetailsSidebarProps> = ({
  selectedImage,
  open,
  setOpen,
  updateImageDescription,
  onDeleteImage,
}) => {
  const [openImageZoomModal, setOpenImageZoomModal] = useState(false);
  const [editingDescription, setEditingDescription] = useState(false);
  const [description, setDescription] = useState(
    selectedImage.imageFile.description ?? "Add a description to this image."
  );
  const handelClose = useCallback(() => {
    setDescription("Add a description to this image.");
    setOpen(false);
  }, [setOpen]);

  useEffect(() => {
    setDescription(
      selectedImage.imageFile.description ?? "Add a description to this image."
    );
  }, [selectedImage]);
 

  return (
    <>
      <Transition.Root show={open} as={Fragment}>
        <Dialog as="div" className="relative z-10" onClose={setOpen}>
          <Transition.Child
            as={Fragment}
            enter="ease-in-out duration-500"
            enterFrom="opacity-0"
            enterTo="opacity-100"
            leave="ease-in-out duration-500"
            leaveFrom="opacity-100"
            leaveTo="opacity-0"
          >
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" />
          </Transition.Child>

          <div className="fixed inset-0 overflow-hidden">
            <div className="absolute inset-0 overflow-hidden">
              <div className="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
                <Transition.Child
                  as={Fragment}
                  enter="transform transition ease-in-out duration-500 sm:duration-700"
                  enterFrom="translate-x-full"
                  enterTo="translate-x-0"
                  leave="transform transition ease-in-out duration-500 sm:duration-700"
                  leaveFrom="translate-x-0"
                  leaveTo="translate-x-full"
                >
                  <Dialog.Panel className="pointer-events-auto relative w-96">
                    <Transition.Child
                      as={Fragment}
                      enter="ease-in-out duration-500"
                      enterFrom="opacity-0"
                      enterTo="opacity-100"
                      leave="ease-in-out duration-500"
                      leaveFrom="opacity-100"
                      leaveTo="opacity-0"
                    >
                      <div className="absolute left-0 top-0 -ml-8 flex pr-2 pt-4 sm:-ml-10 sm:pr-4">
                        <button
                          type="button"
                          className="relative rounded-md text-gray-300 hover:text-white focus:outline-none focus:ring-2 focus:ring-white"
                          onClick={(e) => {
                            e.stopPropagation();
                            handelClose();
                          }}
                        >
                          <span className="absolute -inset-2.5" />
                          <span className="sr-only">Close panel</span>
                          <X className="h-6 w-6" aria-hidden="true" />
                        </button>
                      </div>
                    </Transition.Child>
                    <div className="h-full overflow-y-auto bg-white p-8">
                      <div className="space-y-6 pb-16">
                        <div>
                          <div
                            className="aspect-h-7 aspect-w-10 block w-full overflow-hidden rounded-lg hover:cursor-pointer"
                            onClick={() => setOpenImageZoomModal(true)}
                          >
                            <img
                              src={selectedImage.signedUrl}
                              alt=""
                              className="object-cover"
                            />
                          </div>
                          <div className="mt-4 flex items-start justify-between">
                            <div>
                              <h2 className="text-base font-semibold leading-6 text-gray-900">
                                <span className="sr-only">Details for </span>
                                {selectedImage.imageFile.name.toUpperCase()}
                              </h2>
                              <p className="text-sm font-medium text-gray-500">
                                {selectedImage.imageFile.size.toPrecision(2)} MB
                              </p>
                            </div>
                           
                          </div>
                        </div>
                        <div>
                          <h3 className="font-medium text-gray-900">
                            Information
                          </h3>
                          <dl className="mt-2 divide-y divide-gray-200 border-b border-t border-gray-200">
                            <div className="flex justify-between py-3 text-sm font-medium">
                              <dt className="text-gray-500">Uploaded by</dt>
                              <dd className="text-gray-900">
                                {selectedImage.imageFile.uploadedBy}
                              </dd>
                            </div>
                            <div className="flex justify-between py-3 text-sm font-medium">
                              <dt className="text-gray-500">Created</dt>
                              <dd className="text-gray-900">
                                {DateTime.fromISO(
                                  selectedImage.imageFile.createdAt
                                ).toFormat("DDDD")}
                              </dd>
                            </div>
                            <div className="flex justify-between py-3 text-sm font-medium">
                              <dt className="text-gray-500">Last modified</dt>
                              <dd className="text-gray-900">
                                {DateTime.fromISO(
                                  selectedImage.imageFile.updatedAt
                                ).toFormat("DDDD")}
                              </dd>
                            </div>
                            <div className="flex justify-between py-3 text-sm font-medium">
                              <dt className="text-gray-500">Dimensions</dt>
                              <dd className="text-gray-900">
                                {selectedImage.imageFile.metadata.width} X{" "}
                                {selectedImage.imageFile.metadata.height}
                              </dd>
                            </div>
                          </dl>
                        </div>
                        <div>
                          <h3 className="font-medium text-gray-900">
                            Description
                          </h3>
                          <div className="mt-2 flex items-center justify-between">
                            {!editingDescription ? (
                              <p className="text-sm italic text-gray-500">
                                {description}
                              </p>
                            ) : (
                              <>
                                <textarea
                                  className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                                  value={description}
                                  onChange={(e) => {
                                    e.stopPropagation();
                                    setDescription(e.target.value);
                                  }}
                                  onBlur={(e) => {
                                    e.stopPropagation();
                                    updateImageDescription(
                                      selectedImage.imageFile.id,
                                      description
                                    );
                                    setEditingDescription(false);
                                  }}
                                />
                              </>
                            )}

                            <button
                              type="button"
                              className="relative -mr-2 flex h-8 w-8 items-center justify-center rounded-full bg-white text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                              onClick={(e) => {
                                e.stopPropagation();
                                setEditingDescription(!editingDescription);
                              }}
                            >
                              <span className="absolute -inset-1.5" />
                              <Pen className="h-5 w-5" aria-hidden="true" />
                              <span className="sr-only">Add description</span>
                            </button>
                          </div>
                        </div>

                        <div className="flex">
                           
                          <button
                            type="button"
                            className="ml-3 flex-1 rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
                            onClick={(e) => {
                              onDeleteImage(selectedImage.imageFile.id);
                              handelClose();
                            }}
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    </div>
                  </Dialog.Panel>
                </Transition.Child>
              </div>
            </div>
          </div>
        </Dialog>
      </Transition.Root>
      <ImageZoomModal
        open={openImageZoomModal}
        setOpen={setOpenImageZoomModal}
        imageName={selectedImage.imageFile.name}
        imageUrl={selectedImage.signedUrl}
      />
    </>
  );
};

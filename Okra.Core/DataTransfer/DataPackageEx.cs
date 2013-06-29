﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Okra.DataTransfer
{
    public delegate Task<T> AsyncDataProvider<T>(string formatId, DateTimeOffset deadline);

    public static class DataPackageEx
    {
        // *** Static Methods ***

        public static void SetAsyncDataProvider(this DataPackage dataPackage, string formatId, AsyncDataProvider<object> delayRenderer)
        {
            dataPackage.SetDataProvider(formatId, (DataProviderRequest request) => DataProviderRequestHandler<object>(request, delayRenderer));
        }

        public static void SetAsyncBitmap(this DataPackage dataPackage, AsyncDataProvider<RandomAccessStreamReference> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.Bitmap, (DataProviderRequest request) => DataProviderRequestHandler<RandomAccessStreamReference>(request, delayRenderer));
        }

        public static void SetAsyncHtmlFormat(this DataPackage dataPackage, AsyncDataProvider<string> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.Html, (DataProviderRequest request) => DataProviderRequestHandler<string>(request, delayRenderer));
        }

        public static void SetAsyncRtf(this DataPackage dataPackage, AsyncDataProvider<string> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.Rtf, (DataProviderRequest request) => DataProviderRequestHandler<string>(request, delayRenderer));
        }

        public static void SetAsyncStorageItems(this DataPackage dataPackage, AsyncDataProvider<IEnumerable<IStorageItem>> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.StorageItems, (DataProviderRequest request) => DataProviderRequestHandler<IEnumerable<IStorageItem>>(request, delayRenderer));
        }

        public static void SetAsyncText(this DataPackage dataPackage, AsyncDataProvider<string> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.Text, (DataProviderRequest request) => DataProviderRequestHandler<string>(request, delayRenderer));
        }

        public static void SetAsyncUri(this DataPackage dataPackage, AsyncDataProvider<Uri> delayRenderer)
        {
            dataPackage.SetDataProvider(StandardDataFormats.Uri, (DataProviderRequest request) => DataProviderRequestHandler<Uri>(request, delayRenderer));
        }

        // *** Private Static Methods ***

        private async static void DataProviderRequestHandler<T>(DataProviderRequest request, AsyncDataProvider<T> delayRenderer)
        {
            // Get a deferral for the duration of the request

            DataProviderDeferral deferral = request.GetDeferral();

            // Get the data to return from the data provider

            object data = await delayRenderer(request.FormatId, request.Deadline);
            request.SetData(data);

            // Complete the deferral

            deferral.Complete();
        }
    }
}

--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2
-- Dumped by pg_dump version 17.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE ONLY public."ReturnOutboundDetails" DROP CONSTRAINT "FK_ReturnOutboundDetails_OutboundDetails_OutboundDetailsId";
ALTER TABLE ONLY public."ReturnOutboundDetails" DROP CONSTRAINT "FK_ReturnOutboundDetails_InboundDetails_InboundDetailsId";
ALTER TABLE ONLY public."ProviderAssets" DROP CONSTRAINT "FK_ProviderAssets_Providers_ProviderId";
ALTER TABLE ONLY public."ProviderAssets" DROP CONSTRAINT "FK_ProviderAssets_Assets_AssetId";
ALTER TABLE ONLY public."Products" DROP CONSTRAINT "FK_Products_Providers_ProviderId";
ALTER TABLE ONLY public."ProductCategories" DROP CONSTRAINT "FK_ProductCategories_Products_ProductId";
ALTER TABLE ONLY public."ProductCategories" DROP CONSTRAINT "FK_ProductCategories_Categories_CategoriesId";
ALTER TABLE ONLY public."Outbounds" DROP CONSTRAINT "FK_Outbounds_Customers_CustomerId";
ALTER TABLE ONLY public."Outbounds" DROP CONSTRAINT "FK_Outbounds_Accounts_AccountId";
ALTER TABLE ONLY public."OutboundDetails" DROP CONSTRAINT "FK_OutboundDetails_Outbounds_OutboundId";
ALTER TABLE ONLY public."OutboundDetails" DROP CONSTRAINT "FK_OutboundDetails_Lots_LotId";
ALTER TABLE ONLY public."Notifications" DROP CONSTRAINT "FK_Notifications_Accounts_AccountId";
ALTER TABLE ONLY public."Lots" DROP CONSTRAINT "FK_Lots_Warehouses_WarehouseId";
ALTER TABLE ONLY public."Lots" DROP CONSTRAINT "FK_Lots_Providers_ProviderId";
ALTER TABLE ONLY public."Lots" DROP CONSTRAINT "FK_Lots_Products_ProductId";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "FK_LotTransfers_Warehouses_WarehouseId";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "FK_LotTransfers_Warehouses_ToWareHouseId";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "FK_LotTransfers_Warehouses_FromWareHouseId";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "FK_LotTransfers_Assets_AssetId";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "FK_LotTransfers_Accounts_AccountId";
ALTER TABLE ONLY public."LotTransferDetails" DROP CONSTRAINT "FK_LotTransferDetails_Products_ProductId";
ALTER TABLE ONLY public."LotTransferDetails" DROP CONSTRAINT "FK_LotTransferDetails_Lots_LotId";
ALTER TABLE ONLY public."LotTransferDetails" DROP CONSTRAINT "FK_LotTransferDetails_LotTransfers_LotTransferId";
ALTER TABLE ONLY public."InventoryTransactions" DROP CONSTRAINT "FK_InventoryTransactions_Lots_LotId";
ALTER TABLE ONLY public."InventoryCheck" DROP CONSTRAINT "FK_InventoryCheck_Warehouses_WarehouseId";
ALTER TABLE ONLY public."InventoryCheck" DROP CONSTRAINT "FK_InventoryCheck_Accounts_AccountId";
ALTER TABLE ONLY public."InventoryCheckDetail" DROP CONSTRAINT "FK_InventoryCheckDetail_Lots_LotId";
ALTER TABLE ONLY public."InventoryCheckDetail" DROP CONSTRAINT "FK_InventoryCheckDetail_InventoryCheck_InventoryCheckId";
ALTER TABLE ONLY public."Inbounds" DROP CONSTRAINT "FK_Inbounds_Warehouses_WarehouseId";
ALTER TABLE ONLY public."Inbounds" DROP CONSTRAINT "FK_Inbounds_Providers_ProviderId";
ALTER TABLE ONLY public."Inbounds" DROP CONSTRAINT "FK_Inbounds_InboundRequests_InboundRequestId";
ALTER TABLE ONLY public."Inbounds" DROP CONSTRAINT "FK_Inbounds_Accounts_AccountId";
ALTER TABLE ONLY public."InboundRequests" DROP CONSTRAINT "FK_InboundRequests_Accounts_AccountId";
ALTER TABLE ONLY public."InboundRequestDetails" DROP CONSTRAINT "FK_InboundRequestDetails_Products_ProductId";
ALTER TABLE ONLY public."InboundRequestDetails" DROP CONSTRAINT "FK_InboundRequestDetails_InboundRequests_InboundRequestId";
ALTER TABLE ONLY public."InboundRequestAssets" DROP CONSTRAINT "FK_InboundRequestAssets_InboundRequests_InboundRequestId";
ALTER TABLE ONLY public."InboundRequestAssets" DROP CONSTRAINT "FK_InboundRequestAssets_Assets_AssetId";
ALTER TABLE ONLY public."InboundReports" DROP CONSTRAINT "FK_InboundReports_Inbounds_InboundId";
ALTER TABLE ONLY public."InboundReports" DROP CONSTRAINT "FK_InboundReports_Accounts_AccountId";
ALTER TABLE ONLY public."InboundReportAssets" DROP CONSTRAINT "FK_InboundReportAssets_InboundReports_InboundReportId";
ALTER TABLE ONLY public."InboundReportAssets" DROP CONSTRAINT "FK_InboundReportAssets_Assets_AssetId";
ALTER TABLE ONLY public."InboundDetails" DROP CONSTRAINT "FK_InboundDetails_Products_ProductId";
ALTER TABLE ONLY public."InboundDetails" DROP CONSTRAINT "FK_InboundDetails_Inbounds_InboundId";
ALTER TABLE ONLY public."Devices" DROP CONSTRAINT "FK_Devices_Accounts_AccountId";
ALTER TABLE ONLY public."Categories" DROP CONSTRAINT "FK_Categories_Categories_ParentCategoryId";
ALTER TABLE ONLY public."AuditLogs" DROP CONSTRAINT "FK_AuditLogs_Accounts_AccountId";
ALTER TABLE ONLY public."Assets" DROP CONSTRAINT "FK_Assets_Categories_CategoryId";
ALTER TABLE ONLY public."Assets" DROP CONSTRAINT "FK_Assets_Accounts_AccountId";
ALTER TABLE ONLY public."Accounts" DROP CONSTRAINT "FK_Accounts_Roles_RoleId";
DROP INDEX public."IX_Warehouse_WarehouseCode";
DROP INDEX public."IX_ReturnOutboundDetails_OutboundDetailsId";
DROP INDEX public."IX_ReturnOutboundDetails_InboundDetailsId";
DROP INDEX public."IX_Provider_PhoneNumber";
DROP INDEX public."IX_Provider_DocumentNumber";
DROP INDEX public."IX_ProviderAssets_ProviderId";
DROP INDEX public."IX_Products_ProviderId";
DROP INDEX public."IX_Products_ProductCode";
DROP INDEX public."IX_ProductCategories_ProductId_CategoriesId";
DROP INDEX public."IX_Outbounds_OutboundCode";
DROP INDEX public."IX_Outbounds_CustomerId";
DROP INDEX public."IX_Outbounds_AccountId";
DROP INDEX public."IX_OutboundDetails_OutboundId";
DROP INDEX public."IX_OutboundDetails_LotId";
DROP INDEX public."IX_Notifications_AccountId";
DROP INDEX public."IX_Lots_WarehouseId";
DROP INDEX public."IX_Lots_ProviderId";
DROP INDEX public."IX_Lots_ProductId";
DROP INDEX public."IX_Lots_LotNumber_ExpiryDate_ProviderId_WarehouseId";
DROP INDEX public."IX_LotTransfers_WarehouseId";
DROP INDEX public."IX_LotTransfers_ToWareHouseId";
DROP INDEX public."IX_LotTransfers_LotTransferCode";
DROP INDEX public."IX_LotTransfers_FromWareHouseId";
DROP INDEX public."IX_LotTransfers_AssetId";
DROP INDEX public."IX_LotTransfers_AccountId";
DROP INDEX public."IX_LotTransfer_LotTransferCode";
DROP INDEX public."IX_LotTransferDetails_ProductId";
DROP INDEX public."IX_LotTransferDetails_LotTransferId";
DROP INDEX public."IX_LotTransferDetails_LotId";
DROP INDEX public."IX_InventoryTransactions_LotId";
DROP INDEX public."IX_InventoryCheck_WarehouseId";
DROP INDEX public."IX_InventoryCheck_AccountId";
DROP INDEX public."IX_InventoryCheckDetail_LotId";
DROP INDEX public."IX_InventoryCheckDetail_InventoryCheckId";
DROP INDEX public."IX_Inbounds_WarehouseId";
DROP INDEX public."IX_Inbounds_ProviderOrderCode";
DROP INDEX public."IX_Inbounds_ProviderId";
DROP INDEX public."IX_Inbounds_InboundRequestId";
DROP INDEX public."IX_Inbounds_InboundCode";
DROP INDEX public."IX_Inbounds_AccountId";
DROP INDEX public."IX_InboundRequests_AccountId";
DROP INDEX public."IX_InboundRequest_InboundRequestCode";
DROP INDEX public."IX_InboundRequestDetails_ProductId";
DROP INDEX public."IX_InboundRequestDetails_InboundRequestId";
DROP INDEX public."IX_InboundRequestAssets_InboundRequestId";
DROP INDEX public."IX_InboundReports_InboundId";
DROP INDEX public."IX_InboundReports_AccountId";
DROP INDEX public."IX_InboundReportAssets_InboundReportId";
DROP INDEX public."IX_InboundDetails_ProductId";
DROP INDEX public."IX_InboundDetails_LotNumber";
DROP INDEX public."IX_InboundDetails_InboundId";
DROP INDEX public."IX_Devices_SerialNumber";
DROP INDEX public."IX_Devices_ApiKey";
DROP INDEX public."IX_Devices_AccountId";
DROP INDEX public."IX_Customers_PhoneNumber";
DROP INDEX public."IX_Customers_Email";
DROP INDEX public."IX_Customers_DocumentNumber";
DROP INDEX public."IX_Categories_ParentCategoryId";
DROP INDEX public."IX_Categories_CategoryName";
DROP INDEX public."IX_AuditLogs_AccountId";
DROP INDEX public."IX_Assets_FileUrl";
DROP INDEX public."IX_Assets_FileName";
DROP INDEX public."IX_Assets_CategoryId";
DROP INDEX public."IX_Assets_AccountId";
DROP INDEX public."IX_Accounts_UserName";
DROP INDEX public."IX_Accounts_TOTPSecretKey";
DROP INDEX public."IX_Accounts_RoleId";
DROP INDEX public."IX_Accounts_PhoneNumber";
DROP INDEX public."IX_Accounts_Email";
ALTER TABLE ONLY public."__EFMigrationsHistory" DROP CONSTRAINT "PK___EFMigrationsHistory";
ALTER TABLE ONLY public."Warehouses" DROP CONSTRAINT "PK_Warehouses";
ALTER TABLE ONLY public."Roles" DROP CONSTRAINT "PK_Roles";
ALTER TABLE ONLY public."ReturnOutboundDetails" DROP CONSTRAINT "PK_ReturnOutboundDetails";
ALTER TABLE ONLY public."Providers" DROP CONSTRAINT "PK_Providers";
ALTER TABLE ONLY public."ProviderAssets" DROP CONSTRAINT "PK_ProviderAssets";
ALTER TABLE ONLY public."Products" DROP CONSTRAINT "PK_Products";
ALTER TABLE ONLY public."ProductCategories" DROP CONSTRAINT "PK_ProductCategories";
ALTER TABLE ONLY public."Outbounds" DROP CONSTRAINT "PK_Outbounds";
ALTER TABLE ONLY public."OutboundDetails" DROP CONSTRAINT "PK_OutboundDetails";
ALTER TABLE ONLY public."Notifications" DROP CONSTRAINT "PK_Notifications";
ALTER TABLE ONLY public."Lots" DROP CONSTRAINT "PK_Lots";
ALTER TABLE ONLY public."LotTransfers" DROP CONSTRAINT "PK_LotTransfers";
ALTER TABLE ONLY public."LotTransferDetails" DROP CONSTRAINT "PK_LotTransferDetails";
ALTER TABLE ONLY public."InventoryTransactions" DROP CONSTRAINT "PK_InventoryTransactions";
ALTER TABLE ONLY public."InventoryCheckDetail" DROP CONSTRAINT "PK_InventoryCheckDetail";
ALTER TABLE ONLY public."InventoryCheck" DROP CONSTRAINT "PK_InventoryCheck";
ALTER TABLE ONLY public."Inbounds" DROP CONSTRAINT "PK_Inbounds";
ALTER TABLE ONLY public."InboundRequests" DROP CONSTRAINT "PK_InboundRequests";
ALTER TABLE ONLY public."InboundRequestDetails" DROP CONSTRAINT "PK_InboundRequestDetails";
ALTER TABLE ONLY public."InboundRequestAssets" DROP CONSTRAINT "PK_InboundRequestAssets";
ALTER TABLE ONLY public."InboundReports" DROP CONSTRAINT "PK_InboundReports";
ALTER TABLE ONLY public."InboundReportAssets" DROP CONSTRAINT "PK_InboundReportAssets";
ALTER TABLE ONLY public."InboundDetails" DROP CONSTRAINT "PK_InboundDetails";
ALTER TABLE ONLY public."Devices" DROP CONSTRAINT "PK_Devices";
ALTER TABLE ONLY public."Customers" DROP CONSTRAINT "PK_Customers";
ALTER TABLE ONLY public."Categories" DROP CONSTRAINT "PK_Categories";
ALTER TABLE ONLY public."AuditLogs" DROP CONSTRAINT "PK_AuditLogs";
ALTER TABLE ONLY public."Assets" DROP CONSTRAINT "PK_Assets";
ALTER TABLE ONLY public."Accounts" DROP CONSTRAINT "PK_Accounts";
DROP TABLE public."__EFMigrationsHistory";
DROP TABLE public."Warehouses";
DROP TABLE public."Roles";
DROP TABLE public."ReturnOutboundDetails";
DROP TABLE public."Providers";
DROP TABLE public."ProviderAssets";
DROP TABLE public."Products";
DROP TABLE public."ProductCategories";
DROP TABLE public."Outbounds";
DROP TABLE public."OutboundDetails";
DROP TABLE public."Notifications";
DROP TABLE public."Lots";
DROP TABLE public."LotTransfers";
DROP TABLE public."LotTransferDetails";
DROP TABLE public."InventoryTransactions";
DROP TABLE public."InventoryCheckDetail";
DROP TABLE public."InventoryCheck";
DROP TABLE public."Inbounds";
DROP TABLE public."InboundRequests";
DROP TABLE public."InboundRequestDetails";
DROP TABLE public."InboundRequestAssets";
DROP TABLE public."InboundReports";
DROP TABLE public."InboundReportAssets";
DROP TABLE public."InboundDetails";
DROP TABLE public."Devices";
DROP TABLE public."Customers";
DROP TABLE public."Categories";
DROP TABLE public."AuditLogs";
DROP TABLE public."Assets";
DROP TABLE public."Accounts";
DROP SCHEMA public;
--
-- Name: public; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA public;


--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Accounts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Accounts" (
    "Id" uuid NOT NULL,
    "FullName" text NOT NULL,
    "Status" integer NOT NULL,
    "TwoFactorAuthenticatorStatus" integer NOT NULL,
    "tOTPSecretKey" bytea,
    "RoleId" integer,
    "OTPCode" text,
    "BackupCode" text,
    "AccountSettings" jsonb,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" character varying(15) NOT NULL,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);


--
-- Name: Assets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Assets" (
    "AssetId" integer NOT NULL,
    "FileUrl" text NOT NULL,
    "FileName" text NOT NULL,
    "FileExtension" text NOT NULL,
    "FileSize" bigint NOT NULL,
    "ContentType" text,
    "UploadedAt" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "AccountId" uuid NOT NULL,
    "CategoryId" integer
);


--
-- Name: Assets_AssetId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Assets" ALTER COLUMN "AssetId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Assets_AssetId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AuditLogs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AuditLogs" (
    "AuditId" integer NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Resource" text NOT NULL,
    "Action" text NOT NULL,
    "Payload" jsonb NOT NULL,
    "AccountId" uuid
);


--
-- Name: AuditLogs_AuditId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."AuditLogs" ALTER COLUMN "AuditId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AuditLogs_AuditId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Categories" (
    "CategoriesId" integer NOT NULL,
    "CategoryName" text NOT NULL,
    "ParentCategoryId" integer,
    "Description" text,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Categories_CategoriesId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Categories" ALTER COLUMN "CategoriesId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Categories_CategoriesId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Customers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Customers" (
    "CustomerId" integer NOT NULL,
    "CustomerName" text NOT NULL,
    "Address" text,
    "PhoneNumber" character varying(15),
    "Email" text,
    "IsLoyal" boolean NOT NULL,
    "DocumentNumber" text NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Customers_CustomerId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Customers" ALTER COLUMN "CustomerId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Customers_CustomerId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Devices; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Devices" (
    "DeviceId" integer NOT NULL,
    "DeviceName" text NOT NULL,
    "SerialNumber" text NOT NULL,
    "DeviceType" text NOT NULL,
    "ApiKey" text NOT NULL,
    "ExpiryDate" timestamp with time zone,
    "IsRevoked" boolean NOT NULL,
    "Status" integer NOT NULL,
    "AccountId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Devices_DeviceId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Devices" ALTER COLUMN "DeviceId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Devices_DeviceId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InboundDetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundDetails" (
    "InboundDetailsId" integer NOT NULL,
    "LotNumber" text NOT NULL,
    "ManufacturingDate" date,
    "ExpiryDate" date,
    "ProductId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "OpeningStock" integer,
    "TotalPrice" numeric NOT NULL,
    "InboundId" integer NOT NULL
);


--
-- Name: InboundDetails_InboundDetailsId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InboundDetails" ALTER COLUMN "InboundDetailsId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InboundDetails_InboundDetailsId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InboundReportAssets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundReportAssets" (
    "InboundReportId" integer NOT NULL,
    "AssetId" integer NOT NULL
);


--
-- Name: InboundReports; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundReports" (
    "InboundReportId" integer NOT NULL,
    "ReportDate" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "ProblemDescription" text NOT NULL,
    "AccountId" uuid NOT NULL,
    "InboundId" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: InboundReports_InboundReportId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InboundReports" ALTER COLUMN "InboundReportId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InboundReports_InboundReportId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InboundRequestAssets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundRequestAssets" (
    "InboundRequestId" integer NOT NULL,
    "AssetId" integer NOT NULL
);


--
-- Name: InboundRequestDetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundRequestDetails" (
    "InboundRequestDetailsId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "TotalPrice" numeric NOT NULL,
    "ProductId" integer NOT NULL,
    "InboundRequestId" integer NOT NULL
);


--
-- Name: InboundRequestDetails_InboundRequestDetailsId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InboundRequestDetails" ALTER COLUMN "InboundRequestDetailsId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InboundRequestDetails_InboundRequestDetailsId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InboundRequests; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InboundRequests" (
    "InboundRequestId" integer NOT NULL,
    "InboundRequestCode" text NOT NULL,
    "Status" integer NOT NULL,
    "Note" text,
    "Price" numeric,
    "AccountId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: InboundRequests_InboundRequestId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InboundRequests" ALTER COLUMN "InboundRequestId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InboundRequests_InboundRequestId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Inbounds; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Inbounds" (
    "InboundId" integer NOT NULL,
    "InboundCode" text,
    "ProviderOrderCode" text,
    "Note" text,
    "InboundDate" timestamp with time zone,
    "Status" integer NOT NULL,
    "ProviderId" integer NOT NULL,
    "AccountId" uuid NOT NULL,
    "WarehouseId" integer NOT NULL,
    "InboundRequestId" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Inbounds_InboundId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Inbounds" ALTER COLUMN "InboundId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Inbounds_InboundId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InventoryCheck; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InventoryCheck" (
    "InventoryCheckId" integer NOT NULL,
    "Title" text NOT NULL,
    "CheckDate" timestamp with time zone NOT NULL,
    "AccountId" uuid NOT NULL,
    "WarehouseId" integer NOT NULL,
    "Notes" text
);


--
-- Name: InventoryCheckDetail; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InventoryCheckDetail" (
    "InventoryCheckDetailId" integer NOT NULL,
    "InventoryCheckId" integer NOT NULL,
    "LotId" integer NOT NULL,
    "Status" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "CheckQuantity" integer,
    "Reason" text NOT NULL,
    "Notes" text
);


--
-- Name: InventoryCheckDetail_InventoryCheckDetailId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InventoryCheckDetail" ALTER COLUMN "InventoryCheckDetailId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InventoryCheckDetail_InventoryCheckDetailId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InventoryCheck_InventoryCheckId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InventoryCheck" ALTER COLUMN "InventoryCheckId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InventoryCheck_InventoryCheckId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: InventoryTransactions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."InventoryTransactions" (
    "Id" integer NOT NULL,
    "LotId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: InventoryTransactions_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."InventoryTransactions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."InventoryTransactions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: LotTransferDetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."LotTransferDetails" (
    "LotTransferDetailId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "LotId" integer NOT NULL,
    "LotTransferId" integer NOT NULL,
    "ProductId" integer
);


--
-- Name: LotTransferDetails_LotTransferDetailId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."LotTransferDetails" ALTER COLUMN "LotTransferDetailId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."LotTransferDetails_LotTransferDetailId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: LotTransfers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."LotTransfers" (
    "LotTransferId" integer NOT NULL,
    "LotTransferCode" text NOT NULL,
    "LotTransferStatus" integer NOT NULL,
    "FromWareHouseId" integer NOT NULL,
    "ToWareHouseId" integer NOT NULL,
    "AccountId" uuid NOT NULL,
    "AssetId" integer,
    "WarehouseId" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: LotTransfers_LotTransferId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."LotTransfers" ALTER COLUMN "LotTransferId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."LotTransfers_LotTransferId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Lots; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Lots" (
    "LotId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "LotNumber" text NOT NULL,
    "ManufacturingDate" date,
    "ExpiryDate" date NOT NULL,
    "WarehouseId" integer NOT NULL,
    "ProviderId" integer NOT NULL,
    "ProductId" integer NOT NULL
);


--
-- Name: Lots_LotId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Lots" ALTER COLUMN "LotId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Lots_LotId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Notifications; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Notifications" (
    "NotificationId" integer NOT NULL,
    "Title" text,
    "Content" text,
    "IsRead" boolean NOT NULL,
    "Type" integer NOT NULL,
    "AccountId" uuid,
    "Role" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Notifications_NotificationId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Notifications" ALTER COLUMN "NotificationId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Notifications_NotificationId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: OutboundDetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."OutboundDetails" (
    "OutboundDetailsId" integer NOT NULL,
    "ExpiryDate" date NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "TotalPrice" numeric NOT NULL,
    "Discount" real NOT NULL,
    "OutboundId" integer NOT NULL,
    "LotId" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: OutboundDetails_OutboundDetailsId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."OutboundDetails" ALTER COLUMN "OutboundDetailsId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."OutboundDetails_OutboundDetailsId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Outbounds; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Outbounds" (
    "OutboundId" integer NOT NULL,
    "OutboundCode" text NOT NULL,
    "CustomerId" integer NOT NULL,
    "ReceiverName" text,
    "ReceiverPhone" text,
    "ReceiverAddress" text,
    "OutboundOrderCode" text,
    "TrackingNumber" text,
    "Note" text,
    "OutboundDate" timestamp with time zone,
    "Status" integer NOT NULL,
    "AccountId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Outbounds_OutboundId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Outbounds" ALTER COLUMN "OutboundId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Outbounds_OutboundId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ProductCategories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProductCategories" (
    "ProductId" integer NOT NULL,
    "CategoriesId" integer NOT NULL
);


--
-- Name: Products; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Products" (
    "ProductId" integer NOT NULL,
    "ProductName" text NOT NULL,
    "ProductCode" text NOT NULL,
    "SKU" text NOT NULL,
    "MadeFrom" text NOT NULL,
    "Status" integer NOT NULL,
    "ProviderId" integer
);


--
-- Name: Products_ProductId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Products" ALTER COLUMN "ProductId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Products_ProductId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ProviderAssets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProviderAssets" (
    "ProviderId" integer NOT NULL,
    "AssetId" integer NOT NULL
);


--
-- Name: Providers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Providers" (
    "ProviderId" integer NOT NULL,
    "ProviderName" text NOT NULL,
    "Address" text NOT NULL,
    "PhoneNumber" text NOT NULL,
    "TaxCode" text,
    "Nationality" text,
    "Email" text NOT NULL,
    "DocumentNumber" text NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Providers_ProviderId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Providers" ALTER COLUMN "ProviderId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Providers_ProviderId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ReturnOutboundDetails; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ReturnOutboundDetails" (
    "ReturnOutboundDetailsId" integer NOT NULL,
    "OutboundDetailsId" integer NOT NULL,
    "ReturnedQuantity" integer NOT NULL,
    "Note" text,
    "InboundDetailsId" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: ReturnOutboundDetails_ReturnOutboundDetailsId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."ReturnOutboundDetails" ALTER COLUMN "ReturnOutboundDetailsId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ReturnOutboundDetails_ReturnOutboundDetailsId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Roles" (
    "RoleId" integer NOT NULL,
    "RoleName" text NOT NULL
);


--
-- Name: Roles_RoleId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Roles" ALTER COLUMN "RoleId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Roles_RoleId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Warehouses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Warehouses" (
    "WarehouseId" integer NOT NULL,
    "WarehouseCode" text NOT NULL,
    "WarehouseName" text NOT NULL,
    "Address" text NOT NULL,
    "Status" integer NOT NULL,
    "DocumentNumber" text NOT NULL
);


--
-- Name: Warehouses_WarehouseId_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Warehouses" ALTER COLUMN "WarehouseId" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Warehouses_WarehouseId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Data for Name: Accounts; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('11111111-1111-1111-1111-111111111111', 'Trần Văn An', 1, 1, NULL, 1, NULL, NULL, NULL, 'admin1', NULL, 'tranvana@example.com', NULL, false, 'AQAAAAIAAYagAAAAEO0fzNwPelk9hvYvyA4tZSohqMvW+Mlj200Mxbtey9sG6Nm6ahRIJ4XXFeJziPJCQQ==', NULL, '11111111-1111-1111-1111-111111111111', '0901234567', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('11111111-1111-1111-1111-111111111112', 'Lê Văn Bình', 1, 1, NULL, 1, NULL, NULL, NULL, 'admin2', NULL, 'levanbinh@example.com', NULL, false, 'AQAAAAIAAYagAAAAEG1BxXI7nobOKMbtb6T6ZYqv2AUjCW7C4u/SDuXIl7PX7jyp1TShOwAkcsQz1mxh/w==', NULL, '11111111-1111-1111-1111-111111111112', '0908765432', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('11111111-1111-1111-1111-111111111113', 'Nguyễn Thúc Cường', 1, 1, NULL, 1, NULL, NULL, NULL, 'admin3', NULL, 'nguyenthucc@example.com', NULL, false, 'AQAAAAIAAYagAAAAEBd+TQflP9kUKWFPsxFzRBtk6USSg2fRF/HyiMOhUVaP1msr1HOifBnu4VduJ0VNfw==', NULL, '11111111-1111-1111-1111-111111111113', '0912345678', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('11111111-1111-1111-1111-111111111114', 'Phạm Thị Diệu', 1, 1, NULL, 1, NULL, NULL, NULL, 'admin4', NULL, 'phamthid@example.com', NULL, false, 'AQAAAAIAAYagAAAAEAFhZQnfi2ilAiAjnvN72kI9CgtHwfTT6EimQwHIgjMkAhiFJYtmF6hHXfco6rgTnw==', NULL, '11111111-1111-1111-1111-111111111114', '0934567890', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('11111111-1111-1111-1111-111111111115', 'Hồ Văn Em', 1, 1, NULL, 1, NULL, NULL, NULL, 'admin5', NULL, 'hovanoe@example.com', NULL, false, 'AQAAAAIAAYagAAAAEMC/EdQlF029cUEZ880VignzECjl/ESWJ7ZFg7/+W9q2HYKDZuIIZG68tsNdOR2osw==', NULL, '11111111-1111-1111-1111-111111111115', '0978901234', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('22222222-2222-2222-2222-222222222221', 'Vương Thị Phượng', 1, 1, NULL, 2, NULL, NULL, NULL, 'manager1', NULL, 'vuongthif@example.com', NULL, false, 'AQAAAAIAAYagAAAAEBfMbC5VD0kfeEjyEyxgMAYBd+bFP3rDpe4O4bP3BkCD4swikwWQIvh4Xz3pYuzGbA==', NULL, '22222222-2222-2222-2222-222222222221', '0909876543', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('22222222-2222-2222-2222-222222222222', 'Đinh Văn Giang', 1, 1, NULL, 2, NULL, NULL, NULL, 'manager2', NULL, 'dinhvangg@example.com', NULL, false, 'AQAAAAIAAYagAAAAEND22YAtuRSfPpU91BlFgZAnOrWl1BQ36GAh177ZSzD9RBCFjIzTZFDKNhPiCaI1FA==', NULL, '22222222-2222-2222-2222-222222222222', '0911223344', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('22222222-2222-2222-2222-222222222223', 'Trần Thanh Hải', 1, 1, NULL, 2, NULL, NULL, NULL, 'manager3', NULL, 'tranthanhh@example.com', NULL, false, 'AQAAAAIAAYagAAAAEHWZS9mZ3ZMnL9ye6a3GskxL98651R7izULDcrn5bmboZBBdqJe9bmEqzEx+PCQfhg==', NULL, '22222222-2222-2222-2222-222222222223', '0933445566', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('22222222-2222-2222-2222-222222222224', 'Lê Minh Hoàng', 1, 1, NULL, 2, NULL, NULL, NULL, 'manager4', NULL, 'leminhi@example.com', NULL, false, 'AQAAAAIAAYagAAAAEI+PVsP7W9KmZg4A0PA/0ISX6AjGaz958Fadq0bLncvB7/Yf6A77Y2xCyuee6aAFDw==', NULL, '22222222-2222-2222-2222-222222222224', '0977889900', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('22222222-2222-2222-2222-222222222225', 'Phạm Bảo Châu', 1, 1, NULL, 2, NULL, NULL, NULL, 'manager5', NULL, 'phambaoj@example.com', NULL, false, 'AQAAAAIAAYagAAAAEEueDHa+fFs3MHn1NtX2oS0JPnwsnuCTBhw/vth+2ABWRwF3fhlXuWJkF8W9QFXPpw==', NULL, '22222222-2222-2222-2222-222222222225', '0906543210', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('33333333-3333-3333-3333-333333333331', 'Nguyễn Mai Khanh', 1, 1, NULL, 3, NULL, NULL, NULL, 'accountant1', NULL, 'nguyenmaik@example.com', NULL, false, 'AQAAAAIAAYagAAAAEFqUKksFIo+5S2mvzreAa9XaXa/jbg3FQLqHBfVce4+uY/LF7/6QxN63VqB+CXyZ2g==', NULL, '33333333-3333-3333-3333-333333333331', '0919283746', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('33333333-3333-3333-3333-333333333332', 'Đỗ Việt Long', 1, 1, NULL, 3, NULL, NULL, NULL, 'accountant2', NULL, 'dovietl@example.com', NULL, false, 'AQAAAAIAAYagAAAAEH2D+HKGnbPCKmfOndeT3FcW/yP4Z+SfXDJzIs15cfT/8nDlHT9Ad/d5Y3l+Cv6GJA==', NULL, '33333333-3333-3333-3333-333333333332', '0935791324', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('33333333-3333-3333-3333-333333333333', 'Lê Thị Minh', 1, 1, NULL, 3, NULL, NULL, NULL, 'accountant3', NULL, 'lethim@example.com', NULL, false, 'AQAAAAIAAYagAAAAEPG/0ZuPSSBQ8+WZgMQLDFINKPM3Yrs5ByXbLGT7QzQySDF8KBsuR9N9b4R9IWrl+Q==', NULL, '33333333-3333-3333-3333-333333333333', '0971357924', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('33333333-3333-3333-3333-333333333334', 'Hoàng Văn Nam', 1, 1, NULL, 3, NULL, NULL, NULL, 'accountant4', NULL, 'hoangvann@example.com', NULL, false, 'AQAAAAIAAYagAAAAEIvI9ByAe+WD63mNIYFEY6KJSk4fE3n5bR4TlukmYoyVDVd9py5fBVOqdc9Ldhi31g==', NULL, '33333333-3333-3333-3333-333333333334', '0902468135', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('33333333-3333-3333-3333-333333333335', 'Trần Toàn', 1, 1, NULL, 3, NULL, NULL, NULL, 'accountant5', NULL, 'trantoan@example.com', NULL, false, 'AQAAAAIAAYagAAAAENX3HTYgD2H98xK8caVDjkqsFi8jAcCmGeDznTZDfFJDkOgNt9eLXzwrxYH8gqmXew==', NULL, '33333333-3333-3333-3333-333333333335', '0938527419', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('44444444-4444-4444-4444-444444444441', 'Nguyễn Phương Anh', 1, 1, NULL, 4, NULL, NULL, NULL, 'saleadmin1', NULL, 'nguyenphuongp@example.com', NULL, false, 'AQAAAAIAAYagAAAAEGlX5KWmBvM20kOR8jZx+0jaLgFw1S2k1ssg7KVQhW3bfv2Nb7WMYfKNH5e0W3k97g==', NULL, '44444444-4444-4444-4444-444444444441', '0914725836', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('44444444-4444-4444-4444-444444444442', 'Lê Hoàng Quân', 1, 1, NULL, 4, NULL, NULL, NULL, 'saleadmin2', NULL, 'lehoangq@example.com', NULL, false, 'AQAAAAIAAYagAAAAEAcB6yW4UI317SuEsVB4t/53K6UnxxUlFrMJy2wY3KxFm4l9uds17HPj80LdFg04kg==', NULL, '44444444-4444-4444-4444-444444444442', '0936987412', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('44444444-4444-4444-4444-444444444443', 'Phạm Thị Hồng', 1, 1, NULL, 4, NULL, NULL, NULL, 'saleadmin3', NULL, 'phamthir@example.com', NULL, false, 'AQAAAAIAAYagAAAAEGHsjjOocv73bEDU9u4mR/aDI1K0IrWy3fpjArQVqrLTo6JVm8oXREXIjMzq7eVwIQ==', NULL, '44444444-4444-4444-4444-444444444443', '0978521496', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('44444444-4444-4444-4444-444444444444', 'Đinh Văn Sơn', 1, 1, NULL, 4, NULL, NULL, NULL, 'saleadmin4', NULL, 'dinhvans@example.com', NULL, false, 'AQAAAAIAAYagAAAAELsgQsekhW8IInCzW6EapjAQfdzLxqhnfYpEpcfymm2tDG2LyVlMBxaJPMaTvFrwJQ==', NULL, '44444444-4444-4444-4444-444444444444', '0905123678', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('44444444-4444-4444-4444-444444444445', 'Nguyễn Tuấn Tú', 1, 1, NULL, 4, NULL, NULL, NULL, 'saleadmin5', NULL, 'nguyentuant@example.com', NULL, false, 'AQAAAAIAAYagAAAAENCh04TI29L4YWmNcCxW76sPT8AMrYASur864BQL9t0PACx3Ydv/bDySDjepiyZF/A==', NULL, '44444444-4444-4444-4444-444444444445', '0939876541', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('55555555-5555-5555-5555-555555555551', 'Lê Văn Út', 1, 1, NULL, 5, NULL, NULL, NULL, 'director1', NULL, 'levanu@example.com', NULL, false, 'AQAAAAIAAYagAAAAEFlYLUngCUa3w7xakAK84mFQQhWoiyXXWcrB5tjfKx8krB0WJ7zSwLwr5K+4z6N3sQ==', NULL, '55555555-5555-5555-5555-555555555551', '0916357892', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('55555555-5555-5555-5555-555555555552', 'Phạm Thị Vân', 1, 1, NULL, 5, NULL, NULL, NULL, 'director2', NULL, 'phamthiv@example.com', NULL, false, 'AQAAAAIAAYagAAAAEMyRvIUr/fMW/xD2kvGjPIXeiSQL7C9NdRPsnJuAxeUj/l4nQcHavY7Uma+5J4zYCQ==', NULL, '55555555-5555-5555-5555-555555555552', '0937485961', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('55555555-5555-5555-5555-555555555553', 'Trịnh Văn Xuân', 1, 1, NULL, 5, NULL, NULL, NULL, 'director3', NULL, 'trinhvanx@example.com', NULL, false, 'AQAAAAIAAYagAAAAEPXrqeq77ADs9Evsy5SQxLVSDJeu3Ae97bqQTj5lDUPUb4w3g+fva3JdXwDsNdC1/g==', NULL, '55555555-5555-5555-5555-555555555553', '0979632581', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('55555555-5555-5555-5555-555555555554', 'Đỗ Hồng Yến', 1, 1, NULL, 5, NULL, NULL, NULL, 'director4', NULL, 'dohongy@example.com', NULL, false, 'AQAAAAIAAYagAAAAEJSMLiZci6E1TuvqeBJsexKa5Euv8GsbeqtPSsmAq0hVrqCeopnd9GoMe0A/Yhj0mg==', NULL, '55555555-5555-5555-5555-555555555554', '0908254796', false, false, NULL, false, 0);
INSERT INTO public."Accounts" ("Id", "FullName", "Status", "TwoFactorAuthenticatorStatus", "tOTPSecretKey", "RoleId", "OTPCode", "BackupCode", "AccountSettings", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('55555555-5555-5555-5555-555555555555', 'Nguyễn Hoàng Duy', 1, 1, NULL, 5, NULL, NULL, NULL, 'director5', NULL, 'nguyenhoangz@example.com', NULL, false, 'AQAAAAIAAYagAAAAEC4QAFmbNl0YqpnuPUEm5Ji3bDNeob6QtBdamxODFixaBWwKDKkkeuLYVmp/f/cefw==', NULL, '55555555-5555-5555-5555-555555555555', '0931598742', false, false, NULL, false, 0);


--
-- Data for Name: Assets; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: AuditLogs; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Categories; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (100, 'Báo cáo', NULL, 'Các loại báo cáo thống kê và phân tích.', 1, '2025-04-28 07:23:13.527587+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (200, 'Thuốc & Dược phẩm', NULL, 'Các loại thuốc và dược phẩm.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (300, 'Thiết bị Y tế', NULL, 'Các thiết bị sử dụng cho mục đích y tế.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (400, 'Chăm sóc Cá nhân & Làm đẹp', NULL, 'Các sản phẩm chăm sóc cá nhân và làm đẹp.', 1, '2025-04-28 07:23:13.527598+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (500, 'Sản phẩm cho Mẹ & Bé', NULL, 'Các sản phẩm dành cho bà mẹ và trẻ em.', 1, '2025-04-28 07:23:13.527617+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (600, 'Khác', NULL, 'Các danh mục sản phẩm khác.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (700, 'Đơn vị tính', NULL, 'Các danh mục sản phẩm khác.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (101, 'Báo cáo Doanh thu', 100, 'Báo cáo về doanh thu bán hàng.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (102, 'Báo cáo Kho', 100, 'Báo cáo về tình trạng và số lượng hàng tồn kho.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (103, 'Báo cáo Bán hàng', 100, 'Báo cáo chi tiết về các giao dịch bán hàng.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (104, 'Báo cáo Lợi nhuận', 100, 'Báo cáo về lợi nhuận thu được.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (201, 'Thuốc kê đơn', 200, 'Thuốc cần có đơn thuốc của bác sĩ.', 1, '2025-04-28 07:23:13.527588+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (202, 'Thuốc không kê đơn', 200, 'Thuốc có thể mua tự do không cần đơn thuốc.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (203, 'Thuốc giảm đau, hạ sốt', 200, 'Thuốc giúp giảm đau và hạ sốt.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (204, 'Thuốc trị cảm cúm', 200, 'Thuốc điều trị các triệu chứng cảm lạnh và cúm.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (205, 'Kháng sinh', 200, 'Thuốc dùng để điều trị nhiễm khuẩn.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (206, 'Kháng sinh phổ rộng', 200, 'Kháng sinh có tác dụng trên nhiều loại vi khuẩn.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (207, 'Vitamin & Thực phẩm chức năng', 200, 'Các sản phẩm bổ sung vitamin và khoáng chất.', 1, '2025-04-28 07:23:13.527589+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (208, 'Vitamin tổng hợp', 200, 'Các loại vitamin chứa nhiều dưỡng chất.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (209, 'Men vi sinh & Hỗ trợ tiêu hóa', 200, 'Các sản phẩm chứa lợi khuẩn và hỗ trợ tiêu hóa.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (301, 'Dụng cụ Chẩn đoán & Theo dõi', 300, 'Thiết bị dùng cho việc chẩn đoán và theo dõi sức khỏe.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (302, 'Máy đo huyết áp', 300, 'Thiết bị đo huyết áp.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (303, 'Nhiệt kế điện tử', 300, 'Thiết bị đo nhiệt độ cơ thể điện tử.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (304, 'Máy đo đường huyết', 300, 'Thiết bị đo lượng đường trong máu.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (305, 'Vật tư Tiêu hao', 300, 'Các vật tư sử dụng một lần trong y tế.', 1, '2025-04-28 07:23:13.52759+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (306, 'Băng gạc & Vật liệu băng bó', 300, 'Các loại băng, gạc và vật liệu dùng để băng bó vết thương.', 1, '2025-04-28 07:23:13.527591+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (307, 'Kim tiêm & Ống tiêm', 300, 'Các loại kim và ống tiêm.', 1, '2025-04-28 07:23:13.527591+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (308, 'Thiết bị Hỗ trợ Vận động', 300, 'Các thiết bị hỗ trợ người có vấn đề về vận động.', 1, '2025-04-28 07:23:13.527592+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (309, 'Nạng & Gậy', 300, 'Các loại nạng và gậy chống.', 1, '2025-04-28 07:23:13.527593+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (310, 'Xe lăn', 300, 'Các loại xe lăn cho người khuyết tật hoặc người già.', 1, '2025-04-28 07:23:13.527598+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (401, 'Chăm sóc Da mặt', 400, 'Các sản phẩm chăm sóc da mặt.', 1, '2025-04-28 07:23:13.527599+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (402, 'Sữa rửa mặt', 400, 'Các loại sữa rửa mặt.', 1, '2025-04-28 07:23:13.527599+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (403, 'Kem dưỡng da', 400, 'Các loại kem dưỡng da.', 1, '2025-04-28 07:23:13.527599+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (404, 'Serum & Tinh chất', 400, 'Các loại serum và tinh chất dưỡng da.', 1, '2025-04-28 07:23:13.527599+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (405, 'Chăm sóc Cơ thể', 400, 'Các sản phẩm chăm sóc cơ thể.', 1, '2025-04-28 07:23:13.527599+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (406, 'Sữa tắm', 400, 'Các loại sữa tắm.', 1, '2025-04-28 07:23:13.527616+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (407, 'Kem dưỡng thể', 400, 'Các loại kem dưỡng thể.', 1, '2025-04-28 07:23:13.527616+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (408, 'Chăm sóc Tóc', 400, 'Các sản phẩm chăm sóc tóc.', 1, '2025-04-28 07:23:13.527616+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (409, 'Dầu gội', 400, 'Các loại dầu gội.', 1, '2025-04-28 07:23:13.527616+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (410, 'Dầu xả', 400, 'Các loại dầu xả.', 1, '2025-04-28 07:23:13.527616+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (411, 'Sản phẩm Chống nắng', 400, 'Các sản phẩm bảo vệ da khỏi tác hại của ánh nắng mặt trời.', 1, '2025-04-28 07:23:13.527617+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (501, 'Sản phẩm cho Mẹ', 500, 'Các sản phẩm dành cho phụ nữ mang thai và sau sinh.', 1, '2025-04-28 07:23:13.527617+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (502, 'Vitamin & Thực phẩm chức năng cho mẹ', 500, 'Vitamin và thực phẩm chức năng dành cho mẹ.', 1, '2025-04-28 07:23:13.527617+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (503, 'Đồ dùng cho mẹ', 500, 'Các đồ dùng cá nhân cho mẹ.', 1, '2025-04-28 07:23:13.527617+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (504, 'Sản phẩm cho Bé', 500, 'Các sản phẩm dành cho trẻ em.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (505, 'Sữa & Thực phẩm cho bé', 500, 'Các loại sữa và thực phẩm dinh dưỡng cho trẻ em.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (506, 'Đồ dùng cho bé', 500, 'Các đồ dùng cá nhân cho trẻ em.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (601, 'Vật tư Y tế Gia đình', 600, 'Các vật tư y tế sử dụng tại nhà.', 1, '2025-04-28 07:23:13.527618+00', NULL);
INSERT INTO public."Categories" ("CategoriesId", "CategoryName", "ParentCategoryId", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES (602, 'Sản phẩm Hỗ trợ Sức khỏe', 600, 'Các sản phẩm hỗ trợ sức khỏe tổng thể.', 1, '2025-04-28 07:23:13.527618+00', NULL);


--
-- Data for Name: Customers; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (1, 'Nguyễn Văn An', '123 Đường Cộng Hòa, Phường 12, Quận Tân Bình, TP. Hồ Chí Minh', '0903123456', 'nguyen.van.an@gmail.com', true, 'KH-NVAN-250409-001', 1, '2025-04-28 07:23:13.527661+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (2, 'Lê Thị Bình', '456 Đường 3 Tháng 2, Phường 10, Quận 10, TP. Hồ Chí Minh', '0938987654', 'le.thi.binh79@yahoo.com.vn', false, 'KH-LTBI-250409-002', 1, '2025-04-28 07:23:13.527662+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (3, 'Trần Minh Cường', '789 Đường Nguyễn Trãi, Phường 8, Quận 5, TP. Hồ Chí Minh', '0919234567', 'minhcuong.tran@fpt.net', true, 'KH-TMCO-250409-003', 1, '2025-04-28 07:23:13.527662+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (4, 'Phạm Thị Diệu Hương', '321 Đường Lê Văn Sỹ, Phường 13, Quận Phú Nhuận, TP. Hồ Chí Minh', '0977889900', 'dieuhuong.pham@vnn.vn', false, 'KH-PTDH-250409-004', 1, '2025-04-28 07:23:13.527662+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (5, 'Hoàng Quốc Việt', '654 Đường Điện Biên Phủ, Phường 11, Quận 3, TP. Hồ Chí Minh', '0908555666', 'hoang.viet.quoc@outlook.com', true, 'KH-HQVI-250409-005', 1, '2025-04-28 07:23:13.527662+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (6, 'Vũ Ngọc Ánh', '987 Đường Cách Mạng Tháng 8, Phường 15, Quận 10, TP. Hồ Chí Minh', '0935123456', 'ngocanh.vu@gmail.com', false, 'KH-VNAN-250409-006', 1, '2025-04-28 07:23:13.527663+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (7, 'Đặng Hoàng Long', '159 Đường Nguyễn Thị Thập, Phường Tân Hưng, Quận 7, TP. Hồ Chí Minh', '0917890123', 'hoanglong.dang@saigonnet.vn', true, 'KH-DHLO-250409-007', 1, '2025-04-28 07:23:13.527663+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (8, 'Bùi Thị Thủy Tiên', '753 Đường Phan Xích Long, Phường 2, Quận Phú Nhuận, TP. Hồ Chí Minh', '0909456789', 'thuytien.bui@hcm.fpt.vn', false, 'KH-BTTH-250409-008', 1, '2025-04-28 07:23:13.527663+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (9, 'Lâm Chấn Khang', '852 Đường Kinh Dương Vương, Phường An Lạc, Quận Bình Tân, TP. Hồ Chí Minh', '0933224466', 'chankhang.lam@vitanet.vn', true, 'KH-LCKH-250409-009', 1, '2025-04-28 07:23:13.527663+00', NULL);
INSERT INTO public."Customers" ("CustomerId", "CustomerName", "Address", "PhoneNumber", "Email", "IsLoyal", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (10, 'Trương Thị Mỹ Linh', '951 Đường Trần Hưng Đạo, Phường 1, Quận 5, TP. Hồ Chí Minh', '0976543210', 'mylinh.truong@hcmtelecom.vn', false, 'KH-TTML-250409-010', 1, '2025-04-28 07:23:13.527663+00', NULL);


--
-- Data for Name: Devices; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundDetails; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundReportAssets; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundReports; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundRequestAssets; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundRequestDetails; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InboundRequests; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Inbounds; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InventoryCheck; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InventoryCheckDetail; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: InventoryTransactions; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: LotTransferDetails; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: LotTransfers; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Lots; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Notifications; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: OutboundDetails; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Outbounds; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: ProductCategories; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Products; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: ProviderAssets; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Providers; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Providers" ("ProviderId", "ProviderName", "Address", "PhoneNumber", "TaxCode", "Nationality", "Email", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (1, 'Công ty CP Dược phẩm OPC', '1017 Hồng Bàng, Phường 12, Quận 6, TP.HCM', '02837517111', '0300369857', NULL, 'info@opcpharma.com', 'OPC-HCM-250409-001', 1, '2025-04-28 07:23:13.527567+00', NULL);
INSERT INTO public."Providers" ("ProviderId", "ProviderName", "Address", "PhoneNumber", "TaxCode", "Nationality", "Email", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (2, 'Công ty CP Dược phẩm Imexpharm - Chi nhánh TP.HCM', 'Số 4 Nguyễn Thị Minh Khai, Phường Đa Kao, Quận 1, TP.HCM', '02838223637', '1400113776-001', NULL, 'hcm.branch@imexpharm.com', 'IMP-HCM-090425-002', 1, '2025-04-28 07:23:13.527568+00', NULL);
INSERT INTO public."Providers" ("ProviderId", "ProviderName", "Address", "PhoneNumber", "TaxCode", "Nationality", "Email", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (3, 'Công ty TNHH MTV Dược phẩm DHG - Chi nhánh TP.HCM', '288 Bis Nguyễn Văn Trỗi, Phường 15, Quận Phú Nhuận, TP.HCM', '02838443114', '1800154789-002', NULL, 'dhghcm@dhgpharma.com.vn', 'DHG-HCM-2025-003', 1, '2025-04-28 07:23:13.527569+00', NULL);
INSERT INTO public."Providers" ("ProviderId", "ProviderName", "Address", "PhoneNumber", "TaxCode", "Nationality", "Email", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (4, 'Công ty CP Pymepharco - Chi nhánh TP.HCM', 'Tầng 5, Tòa nhà Pearl Plaza, 561A Điện Biên Phủ, Phường 25, Quận Bình Thạnh, TP.HCM', '02839708789', '4400236473-003', NULL, 'hcm.sales@pymepharco.com', 'PYM-HCM-040925-004', 1, '2025-04-28 07:23:13.527569+00', NULL);
INSERT INTO public."Providers" ("ProviderId", "ProviderName", "Address", "PhoneNumber", "TaxCode", "Nationality", "Email", "DocumentNumber", "Status", "CreatedAt", "UpdatedAt") VALUES (5, 'Công ty CP Dược phẩm Savi', 'Lô J2-J3-J4, Đường D3, KCN Tây Bắc Củ Chi, TP.HCM', '02837260288', '0302589901', NULL, 'info@savipharm.com', 'SAVI-HCM-250409-005', 1, '2025-04-28 07:23:13.527569+00', NULL);


--
-- Data for Name: ReturnOutboundDetails; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Roles; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Roles" ("RoleId", "RoleName") VALUES (1, 'Admin');
INSERT INTO public."Roles" ("RoleId", "RoleName") VALUES (2, 'Inventory Manager');
INSERT INTO public."Roles" ("RoleId", "RoleName") VALUES (3, 'Accountant');
INSERT INTO public."Roles" ("RoleId", "RoleName") VALUES (4, 'Sale Admin');
INSERT INTO public."Roles" ("RoleId", "RoleName") VALUES (5, 'Director');


--
-- Data for Name: Warehouses; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (1, 'KVN-01', 'Kho Việt Nam', 'Số 10 Đường Cộng Hòa, Phường 13, Quận Tân Bình, TP.HCM', 1, 'K20250409-001');
INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (2, 'KHUY-01', 'Kho Hủy', 'Khu vực xử lý hàng lỗi, Đường Số 7, KCN Vĩnh Lộc, Bình Chánh, TP.HCM', 1, 'KH20250409-002');
INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (3, 'KTHU-01', 'Kho Thuốc', 'Số 3B Đường Nguyễn Văn Quá, Đông Hưng Thuận, Quận 12, TP.HCM', 1, 'KT20250409-003');
INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (4, 'KMP-01', 'Kho Mỹ Phẩm', 'Số 1 Lê Duẩn, Bến Nghé, Quận 1, TP.HCM', 1, 'KMP20250409-004');
INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (5, 'KTH-01', 'Kho Trung Hạnh', 'Số 88 Đường 3 Tháng 2, Phường 11, Quận 10, TP.HCM', 1, 'KTH20250409-005');
INSERT INTO public."Warehouses" ("WarehouseId", "WarehouseCode", "WarehouseName", "Address", "Status", "DocumentNumber") VALUES (6, 'KTAMTHOI-01', 'Kho Tạm', 'Số 95 Đường 3 Tháng 2, Phường 11, Quận 10, TP.HCM', 1, 'KTAMTHOI20250409-006');


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20250428072314_InitialCreateV67', '8.0.13');


--
-- Name: Assets_AssetId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Assets_AssetId_seq"', 1, false);


--
-- Name: AuditLogs_AuditId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AuditLogs_AuditId_seq"', 1, false);


--
-- Name: Categories_CategoriesId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Categories_CategoriesId_seq"', 701, false);


--
-- Name: Customers_CustomerId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Customers_CustomerId_seq"', 11, false);


--
-- Name: Devices_DeviceId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Devices_DeviceId_seq"', 1, false);


--
-- Name: InboundDetails_InboundDetailsId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InboundDetails_InboundDetailsId_seq"', 1, false);


--
-- Name: InboundReports_InboundReportId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InboundReports_InboundReportId_seq"', 1, false);


--
-- Name: InboundRequestDetails_InboundRequestDetailsId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InboundRequestDetails_InboundRequestDetailsId_seq"', 1, false);


--
-- Name: InboundRequests_InboundRequestId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InboundRequests_InboundRequestId_seq"', 1, false);


--
-- Name: Inbounds_InboundId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Inbounds_InboundId_seq"', 1, false);


--
-- Name: InventoryCheckDetail_InventoryCheckDetailId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InventoryCheckDetail_InventoryCheckDetailId_seq"', 1, false);


--
-- Name: InventoryCheck_InventoryCheckId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InventoryCheck_InventoryCheckId_seq"', 1, false);


--
-- Name: InventoryTransactions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."InventoryTransactions_Id_seq"', 1, false);


--
-- Name: LotTransferDetails_LotTransferDetailId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."LotTransferDetails_LotTransferDetailId_seq"', 1, false);


--
-- Name: LotTransfers_LotTransferId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."LotTransfers_LotTransferId_seq"', 1, false);


--
-- Name: Lots_LotId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Lots_LotId_seq"', 1, false);


--
-- Name: Notifications_NotificationId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Notifications_NotificationId_seq"', 1, false);


--
-- Name: OutboundDetails_OutboundDetailsId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."OutboundDetails_OutboundDetailsId_seq"', 1, false);


--
-- Name: Outbounds_OutboundId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Outbounds_OutboundId_seq"', 1, false);


--
-- Name: Products_ProductId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Products_ProductId_seq"', 1, false);


--
-- Name: Providers_ProviderId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Providers_ProviderId_seq"', 6, false);


--
-- Name: ReturnOutboundDetails_ReturnOutboundDetailsId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."ReturnOutboundDetails_ReturnOutboundDetailsId_seq"', 1, false);


--
-- Name: Roles_RoleId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Roles_RoleId_seq"', 6, false);


--
-- Name: Warehouses_WarehouseId_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Warehouses_WarehouseId_seq"', 7, false);


--
-- Name: Accounts PK_Accounts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Accounts"
    ADD CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id");


--
-- Name: Assets PK_Assets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Assets"
    ADD CONSTRAINT "PK_Assets" PRIMARY KEY ("AssetId");


--
-- Name: AuditLogs PK_AuditLogs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("AuditId");


--
-- Name: Categories PK_Categories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "PK_Categories" PRIMARY KEY ("CategoriesId");


--
-- Name: Customers PK_Customers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Customers"
    ADD CONSTRAINT "PK_Customers" PRIMARY KEY ("CustomerId");


--
-- Name: Devices PK_Devices; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Devices"
    ADD CONSTRAINT "PK_Devices" PRIMARY KEY ("DeviceId");


--
-- Name: InboundDetails PK_InboundDetails; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundDetails"
    ADD CONSTRAINT "PK_InboundDetails" PRIMARY KEY ("InboundDetailsId");


--
-- Name: InboundReportAssets PK_InboundReportAssets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReportAssets"
    ADD CONSTRAINT "PK_InboundReportAssets" PRIMARY KEY ("AssetId", "InboundReportId");


--
-- Name: InboundReports PK_InboundReports; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReports"
    ADD CONSTRAINT "PK_InboundReports" PRIMARY KEY ("InboundReportId");


--
-- Name: InboundRequestAssets PK_InboundRequestAssets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestAssets"
    ADD CONSTRAINT "PK_InboundRequestAssets" PRIMARY KEY ("AssetId", "InboundRequestId");


--
-- Name: InboundRequestDetails PK_InboundRequestDetails; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestDetails"
    ADD CONSTRAINT "PK_InboundRequestDetails" PRIMARY KEY ("InboundRequestDetailsId");


--
-- Name: InboundRequests PK_InboundRequests; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequests"
    ADD CONSTRAINT "PK_InboundRequests" PRIMARY KEY ("InboundRequestId");


--
-- Name: Inbounds PK_Inbounds; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Inbounds"
    ADD CONSTRAINT "PK_Inbounds" PRIMARY KEY ("InboundId");


--
-- Name: InventoryCheck PK_InventoryCheck; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheck"
    ADD CONSTRAINT "PK_InventoryCheck" PRIMARY KEY ("InventoryCheckId");


--
-- Name: InventoryCheckDetail PK_InventoryCheckDetail; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheckDetail"
    ADD CONSTRAINT "PK_InventoryCheckDetail" PRIMARY KEY ("InventoryCheckDetailId");


--
-- Name: InventoryTransactions PK_InventoryTransactions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "PK_InventoryTransactions" PRIMARY KEY ("Id");


--
-- Name: LotTransferDetails PK_LotTransferDetails; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransferDetails"
    ADD CONSTRAINT "PK_LotTransferDetails" PRIMARY KEY ("LotTransferDetailId");


--
-- Name: LotTransfers PK_LotTransfers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "PK_LotTransfers" PRIMARY KEY ("LotTransferId");


--
-- Name: Lots PK_Lots; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lots"
    ADD CONSTRAINT "PK_Lots" PRIMARY KEY ("LotId");


--
-- Name: Notifications PK_Notifications; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "PK_Notifications" PRIMARY KEY ("NotificationId");


--
-- Name: OutboundDetails PK_OutboundDetails; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OutboundDetails"
    ADD CONSTRAINT "PK_OutboundDetails" PRIMARY KEY ("OutboundDetailsId");


--
-- Name: Outbounds PK_Outbounds; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Outbounds"
    ADD CONSTRAINT "PK_Outbounds" PRIMARY KEY ("OutboundId");


--
-- Name: ProductCategories PK_ProductCategories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProductCategories"
    ADD CONSTRAINT "PK_ProductCategories" PRIMARY KEY ("CategoriesId", "ProductId");


--
-- Name: Products PK_Products; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "PK_Products" PRIMARY KEY ("ProductId");


--
-- Name: ProviderAssets PK_ProviderAssets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProviderAssets"
    ADD CONSTRAINT "PK_ProviderAssets" PRIMARY KEY ("AssetId", "ProviderId");


--
-- Name: Providers PK_Providers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Providers"
    ADD CONSTRAINT "PK_Providers" PRIMARY KEY ("ProviderId");


--
-- Name: ReturnOutboundDetails PK_ReturnOutboundDetails; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReturnOutboundDetails"
    ADD CONSTRAINT "PK_ReturnOutboundDetails" PRIMARY KEY ("ReturnOutboundDetailsId");


--
-- Name: Roles PK_Roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "PK_Roles" PRIMARY KEY ("RoleId");


--
-- Name: Warehouses PK_Warehouses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Warehouses"
    ADD CONSTRAINT "PK_Warehouses" PRIMARY KEY ("WarehouseId");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_Accounts_Email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Accounts_Email" ON public."Accounts" USING btree ("Email");


--
-- Name: IX_Accounts_PhoneNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Accounts_PhoneNumber" ON public."Accounts" USING btree ("PhoneNumber");


--
-- Name: IX_Accounts_RoleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Accounts_RoleId" ON public."Accounts" USING btree ("RoleId");


--
-- Name: IX_Accounts_TOTPSecretKey; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Accounts_TOTPSecretKey" ON public."Accounts" USING btree ("tOTPSecretKey" DESC);


--
-- Name: IX_Accounts_UserName; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Accounts_UserName" ON public."Accounts" USING btree ("UserName");


--
-- Name: IX_Assets_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Assets_AccountId" ON public."Assets" USING btree ("AccountId");


--
-- Name: IX_Assets_CategoryId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Assets_CategoryId" ON public."Assets" USING btree ("CategoryId");


--
-- Name: IX_Assets_FileName; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Assets_FileName" ON public."Assets" USING btree ("FileName");


--
-- Name: IX_Assets_FileUrl; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Assets_FileUrl" ON public."Assets" USING btree ("FileUrl");


--
-- Name: IX_AuditLogs_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_AuditLogs_AccountId" ON public."AuditLogs" USING btree ("AccountId");


--
-- Name: IX_Categories_CategoryName; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Categories_CategoryName" ON public."Categories" USING btree ("CategoryName");


--
-- Name: IX_Categories_ParentCategoryId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Categories_ParentCategoryId" ON public."Categories" USING btree ("ParentCategoryId");


--
-- Name: IX_Customers_DocumentNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Customers_DocumentNumber" ON public."Customers" USING btree ("DocumentNumber");


--
-- Name: IX_Customers_Email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Customers_Email" ON public."Customers" USING btree ("Email");


--
-- Name: IX_Customers_PhoneNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Customers_PhoneNumber" ON public."Customers" USING btree ("PhoneNumber");


--
-- Name: IX_Devices_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Devices_AccountId" ON public."Devices" USING btree ("AccountId");


--
-- Name: IX_Devices_ApiKey; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Devices_ApiKey" ON public."Devices" USING btree ("ApiKey");


--
-- Name: IX_Devices_SerialNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Devices_SerialNumber" ON public."Devices" USING btree ("SerialNumber");


--
-- Name: IX_InboundDetails_InboundId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundDetails_InboundId" ON public."InboundDetails" USING btree ("InboundId");


--
-- Name: IX_InboundDetails_LotNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundDetails_LotNumber" ON public."InboundDetails" USING btree ("LotNumber");


--
-- Name: IX_InboundDetails_ProductId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundDetails_ProductId" ON public."InboundDetails" USING btree ("ProductId");


--
-- Name: IX_InboundReportAssets_InboundReportId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundReportAssets_InboundReportId" ON public."InboundReportAssets" USING btree ("InboundReportId");


--
-- Name: IX_InboundReports_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundReports_AccountId" ON public."InboundReports" USING btree ("AccountId");


--
-- Name: IX_InboundReports_InboundId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundReports_InboundId" ON public."InboundReports" USING btree ("InboundId");


--
-- Name: IX_InboundRequestAssets_InboundRequestId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundRequestAssets_InboundRequestId" ON public."InboundRequestAssets" USING btree ("InboundRequestId");


--
-- Name: IX_InboundRequestDetails_InboundRequestId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundRequestDetails_InboundRequestId" ON public."InboundRequestDetails" USING btree ("InboundRequestId");


--
-- Name: IX_InboundRequestDetails_ProductId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundRequestDetails_ProductId" ON public."InboundRequestDetails" USING btree ("ProductId");


--
-- Name: IX_InboundRequest_InboundRequestCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_InboundRequest_InboundRequestCode" ON public."InboundRequests" USING btree ("InboundRequestCode");


--
-- Name: IX_InboundRequests_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InboundRequests_AccountId" ON public."InboundRequests" USING btree ("AccountId");


--
-- Name: IX_Inbounds_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Inbounds_AccountId" ON public."Inbounds" USING btree ("AccountId");


--
-- Name: IX_Inbounds_InboundCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Inbounds_InboundCode" ON public."Inbounds" USING btree ("InboundCode");


--
-- Name: IX_Inbounds_InboundRequestId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Inbounds_InboundRequestId" ON public."Inbounds" USING btree ("InboundRequestId");


--
-- Name: IX_Inbounds_ProviderId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Inbounds_ProviderId" ON public."Inbounds" USING btree ("ProviderId");


--
-- Name: IX_Inbounds_ProviderOrderCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Inbounds_ProviderOrderCode" ON public."Inbounds" USING btree ("ProviderOrderCode");


--
-- Name: IX_Inbounds_WarehouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Inbounds_WarehouseId" ON public."Inbounds" USING btree ("WarehouseId");


--
-- Name: IX_InventoryCheckDetail_InventoryCheckId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InventoryCheckDetail_InventoryCheckId" ON public."InventoryCheckDetail" USING btree ("InventoryCheckId");


--
-- Name: IX_InventoryCheckDetail_LotId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InventoryCheckDetail_LotId" ON public."InventoryCheckDetail" USING btree ("LotId");


--
-- Name: IX_InventoryCheck_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InventoryCheck_AccountId" ON public."InventoryCheck" USING btree ("AccountId");


--
-- Name: IX_InventoryCheck_WarehouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InventoryCheck_WarehouseId" ON public."InventoryCheck" USING btree ("WarehouseId");


--
-- Name: IX_InventoryTransactions_LotId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_InventoryTransactions_LotId" ON public."InventoryTransactions" USING btree ("LotId");


--
-- Name: IX_LotTransferDetails_LotId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransferDetails_LotId" ON public."LotTransferDetails" USING btree ("LotId");


--
-- Name: IX_LotTransferDetails_LotTransferId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransferDetails_LotTransferId" ON public."LotTransferDetails" USING btree ("LotTransferId");


--
-- Name: IX_LotTransferDetails_ProductId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransferDetails_ProductId" ON public."LotTransferDetails" USING btree ("ProductId");


--
-- Name: IX_LotTransfer_LotTransferCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_LotTransfer_LotTransferCode" ON public."LotTransfers" USING btree ("LotTransferCode");


--
-- Name: IX_LotTransfers_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransfers_AccountId" ON public."LotTransfers" USING btree ("AccountId");


--
-- Name: IX_LotTransfers_AssetId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_LotTransfers_AssetId" ON public."LotTransfers" USING btree ("AssetId");


--
-- Name: IX_LotTransfers_FromWareHouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransfers_FromWareHouseId" ON public."LotTransfers" USING btree ("FromWareHouseId");


--
-- Name: IX_LotTransfers_LotTransferCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_LotTransfers_LotTransferCode" ON public."LotTransfers" USING btree ("LotTransferCode");


--
-- Name: IX_LotTransfers_ToWareHouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransfers_ToWareHouseId" ON public."LotTransfers" USING btree ("ToWareHouseId");


--
-- Name: IX_LotTransfers_WarehouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_LotTransfers_WarehouseId" ON public."LotTransfers" USING btree ("WarehouseId");


--
-- Name: IX_Lots_LotNumber_ExpiryDate_ProviderId_WarehouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Lots_LotNumber_ExpiryDate_ProviderId_WarehouseId" ON public."Lots" USING btree ("LotNumber", "ExpiryDate", "ProviderId", "WarehouseId");


--
-- Name: IX_Lots_ProductId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Lots_ProductId" ON public."Lots" USING btree ("ProductId");


--
-- Name: IX_Lots_ProviderId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Lots_ProviderId" ON public."Lots" USING btree ("ProviderId");


--
-- Name: IX_Lots_WarehouseId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Lots_WarehouseId" ON public."Lots" USING btree ("WarehouseId");


--
-- Name: IX_Notifications_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Notifications_AccountId" ON public."Notifications" USING btree ("AccountId");


--
-- Name: IX_OutboundDetails_LotId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_OutboundDetails_LotId" ON public."OutboundDetails" USING btree ("LotId");


--
-- Name: IX_OutboundDetails_OutboundId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_OutboundDetails_OutboundId" ON public."OutboundDetails" USING btree ("OutboundId");


--
-- Name: IX_Outbounds_AccountId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Outbounds_AccountId" ON public."Outbounds" USING btree ("AccountId");


--
-- Name: IX_Outbounds_CustomerId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Outbounds_CustomerId" ON public."Outbounds" USING btree ("CustomerId");


--
-- Name: IX_Outbounds_OutboundCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Outbounds_OutboundCode" ON public."Outbounds" USING btree ("OutboundCode");


--
-- Name: IX_ProductCategories_ProductId_CategoriesId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_ProductCategories_ProductId_CategoriesId" ON public."ProductCategories" USING btree ("ProductId", "CategoriesId");


--
-- Name: IX_Products_ProductCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Products_ProductCode" ON public."Products" USING btree ("ProductCode");


--
-- Name: IX_Products_ProviderId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Products_ProviderId" ON public."Products" USING btree ("ProviderId");


--
-- Name: IX_ProviderAssets_ProviderId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ProviderAssets_ProviderId" ON public."ProviderAssets" USING btree ("ProviderId");


--
-- Name: IX_Provider_DocumentNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Provider_DocumentNumber" ON public."Providers" USING btree ("DocumentNumber");


--
-- Name: IX_Provider_PhoneNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Provider_PhoneNumber" ON public."Providers" USING btree ("PhoneNumber");


--
-- Name: IX_ReturnOutboundDetails_InboundDetailsId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ReturnOutboundDetails_InboundDetailsId" ON public."ReturnOutboundDetails" USING btree ("InboundDetailsId");


--
-- Name: IX_ReturnOutboundDetails_OutboundDetailsId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ReturnOutboundDetails_OutboundDetailsId" ON public."ReturnOutboundDetails" USING btree ("OutboundDetailsId");


--
-- Name: IX_Warehouse_WarehouseCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Warehouse_WarehouseCode" ON public."Warehouses" USING btree ("WarehouseCode");


--
-- Name: Accounts FK_Accounts_Roles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Accounts"
    ADD CONSTRAINT "FK_Accounts_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("RoleId");


--
-- Name: Assets FK_Assets_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Assets"
    ADD CONSTRAINT "FK_Assets_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: Assets FK_Assets_Categories_CategoryId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Assets"
    ADD CONSTRAINT "FK_Assets_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES public."Categories"("CategoriesId");


--
-- Name: AuditLogs FK_AuditLogs_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "FK_AuditLogs_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id");


--
-- Name: Categories FK_Categories_Categories_ParentCategoryId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "FK_Categories_Categories_ParentCategoryId" FOREIGN KEY ("ParentCategoryId") REFERENCES public."Categories"("CategoriesId") ON DELETE RESTRICT;


--
-- Name: Devices FK_Devices_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Devices"
    ADD CONSTRAINT "FK_Devices_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: InboundDetails FK_InboundDetails_Inbounds_InboundId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundDetails"
    ADD CONSTRAINT "FK_InboundDetails_Inbounds_InboundId" FOREIGN KEY ("InboundId") REFERENCES public."Inbounds"("InboundId") ON DELETE CASCADE;


--
-- Name: InboundDetails FK_InboundDetails_Products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundDetails"
    ADD CONSTRAINT "FK_InboundDetails_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("ProductId") ON DELETE CASCADE;


--
-- Name: InboundReportAssets FK_InboundReportAssets_Assets_AssetId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReportAssets"
    ADD CONSTRAINT "FK_InboundReportAssets_Assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES public."Assets"("AssetId") ON DELETE CASCADE;


--
-- Name: InboundReportAssets FK_InboundReportAssets_InboundReports_InboundReportId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReportAssets"
    ADD CONSTRAINT "FK_InboundReportAssets_InboundReports_InboundReportId" FOREIGN KEY ("InboundReportId") REFERENCES public."InboundReports"("InboundReportId") ON DELETE CASCADE;


--
-- Name: InboundReports FK_InboundReports_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReports"
    ADD CONSTRAINT "FK_InboundReports_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: InboundReports FK_InboundReports_Inbounds_InboundId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundReports"
    ADD CONSTRAINT "FK_InboundReports_Inbounds_InboundId" FOREIGN KEY ("InboundId") REFERENCES public."Inbounds"("InboundId") ON DELETE CASCADE;


--
-- Name: InboundRequestAssets FK_InboundRequestAssets_Assets_AssetId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestAssets"
    ADD CONSTRAINT "FK_InboundRequestAssets_Assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES public."Assets"("AssetId") ON DELETE CASCADE;


--
-- Name: InboundRequestAssets FK_InboundRequestAssets_InboundRequests_InboundRequestId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestAssets"
    ADD CONSTRAINT "FK_InboundRequestAssets_InboundRequests_InboundRequestId" FOREIGN KEY ("InboundRequestId") REFERENCES public."InboundRequests"("InboundRequestId") ON DELETE CASCADE;


--
-- Name: InboundRequestDetails FK_InboundRequestDetails_InboundRequests_InboundRequestId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestDetails"
    ADD CONSTRAINT "FK_InboundRequestDetails_InboundRequests_InboundRequestId" FOREIGN KEY ("InboundRequestId") REFERENCES public."InboundRequests"("InboundRequestId") ON DELETE CASCADE;


--
-- Name: InboundRequestDetails FK_InboundRequestDetails_Products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequestDetails"
    ADD CONSTRAINT "FK_InboundRequestDetails_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("ProductId") ON DELETE CASCADE;


--
-- Name: InboundRequests FK_InboundRequests_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InboundRequests"
    ADD CONSTRAINT "FK_InboundRequests_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: Inbounds FK_Inbounds_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Inbounds"
    ADD CONSTRAINT "FK_Inbounds_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: Inbounds FK_Inbounds_InboundRequests_InboundRequestId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Inbounds"
    ADD CONSTRAINT "FK_Inbounds_InboundRequests_InboundRequestId" FOREIGN KEY ("InboundRequestId") REFERENCES public."InboundRequests"("InboundRequestId");


--
-- Name: Inbounds FK_Inbounds_Providers_ProviderId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Inbounds"
    ADD CONSTRAINT "FK_Inbounds_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES public."Providers"("ProviderId") ON DELETE CASCADE;


--
-- Name: Inbounds FK_Inbounds_Warehouses_WarehouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Inbounds"
    ADD CONSTRAINT "FK_Inbounds_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES public."Warehouses"("WarehouseId") ON DELETE CASCADE;


--
-- Name: InventoryCheckDetail FK_InventoryCheckDetail_InventoryCheck_InventoryCheckId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheckDetail"
    ADD CONSTRAINT "FK_InventoryCheckDetail_InventoryCheck_InventoryCheckId" FOREIGN KEY ("InventoryCheckId") REFERENCES public."InventoryCheck"("InventoryCheckId") ON DELETE CASCADE;


--
-- Name: InventoryCheckDetail FK_InventoryCheckDetail_Lots_LotId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheckDetail"
    ADD CONSTRAINT "FK_InventoryCheckDetail_Lots_LotId" FOREIGN KEY ("LotId") REFERENCES public."Lots"("LotId") ON DELETE CASCADE;


--
-- Name: InventoryCheck FK_InventoryCheck_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheck"
    ADD CONSTRAINT "FK_InventoryCheck_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: InventoryCheck FK_InventoryCheck_Warehouses_WarehouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryCheck"
    ADD CONSTRAINT "FK_InventoryCheck_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES public."Warehouses"("WarehouseId") ON DELETE CASCADE;


--
-- Name: InventoryTransactions FK_InventoryTransactions_Lots_LotId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_Lots_LotId" FOREIGN KEY ("LotId") REFERENCES public."Lots"("LotId") ON DELETE CASCADE;


--
-- Name: LotTransferDetails FK_LotTransferDetails_LotTransfers_LotTransferId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransferDetails"
    ADD CONSTRAINT "FK_LotTransferDetails_LotTransfers_LotTransferId" FOREIGN KEY ("LotTransferId") REFERENCES public."LotTransfers"("LotTransferId") ON DELETE CASCADE;


--
-- Name: LotTransferDetails FK_LotTransferDetails_Lots_LotId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransferDetails"
    ADD CONSTRAINT "FK_LotTransferDetails_Lots_LotId" FOREIGN KEY ("LotId") REFERENCES public."Lots"("LotId") ON DELETE CASCADE;


--
-- Name: LotTransferDetails FK_LotTransferDetails_Products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransferDetails"
    ADD CONSTRAINT "FK_LotTransferDetails_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("ProductId");


--
-- Name: LotTransfers FK_LotTransfers_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "FK_LotTransfers_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: LotTransfers FK_LotTransfers_Assets_AssetId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "FK_LotTransfers_Assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES public."Assets"("AssetId");


--
-- Name: LotTransfers FK_LotTransfers_Warehouses_FromWareHouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "FK_LotTransfers_Warehouses_FromWareHouseId" FOREIGN KEY ("FromWareHouseId") REFERENCES public."Warehouses"("WarehouseId") ON DELETE CASCADE;


--
-- Name: LotTransfers FK_LotTransfers_Warehouses_ToWareHouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "FK_LotTransfers_Warehouses_ToWareHouseId" FOREIGN KEY ("ToWareHouseId") REFERENCES public."Warehouses"("WarehouseId") ON DELETE CASCADE;


--
-- Name: LotTransfers FK_LotTransfers_Warehouses_WarehouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."LotTransfers"
    ADD CONSTRAINT "FK_LotTransfers_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES public."Warehouses"("WarehouseId");


--
-- Name: Lots FK_Lots_Products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lots"
    ADD CONSTRAINT "FK_Lots_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("ProductId") ON DELETE CASCADE;


--
-- Name: Lots FK_Lots_Providers_ProviderId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lots"
    ADD CONSTRAINT "FK_Lots_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES public."Providers"("ProviderId") ON DELETE CASCADE;


--
-- Name: Lots FK_Lots_Warehouses_WarehouseId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Lots"
    ADD CONSTRAINT "FK_Lots_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES public."Warehouses"("WarehouseId") ON DELETE CASCADE;


--
-- Name: Notifications FK_Notifications_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id");


--
-- Name: OutboundDetails FK_OutboundDetails_Lots_LotId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OutboundDetails"
    ADD CONSTRAINT "FK_OutboundDetails_Lots_LotId" FOREIGN KEY ("LotId") REFERENCES public."Lots"("LotId") ON DELETE CASCADE;


--
-- Name: OutboundDetails FK_OutboundDetails_Outbounds_OutboundId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."OutboundDetails"
    ADD CONSTRAINT "FK_OutboundDetails_Outbounds_OutboundId" FOREIGN KEY ("OutboundId") REFERENCES public."Outbounds"("OutboundId") ON DELETE CASCADE;


--
-- Name: Outbounds FK_Outbounds_Accounts_AccountId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Outbounds"
    ADD CONSTRAINT "FK_Outbounds_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES public."Accounts"("Id") ON DELETE CASCADE;


--
-- Name: Outbounds FK_Outbounds_Customers_CustomerId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Outbounds"
    ADD CONSTRAINT "FK_Outbounds_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES public."Customers"("CustomerId") ON DELETE CASCADE;


--
-- Name: ProductCategories FK_ProductCategories_Categories_CategoriesId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProductCategories"
    ADD CONSTRAINT "FK_ProductCategories_Categories_CategoriesId" FOREIGN KEY ("CategoriesId") REFERENCES public."Categories"("CategoriesId") ON DELETE CASCADE;


--
-- Name: ProductCategories FK_ProductCategories_Products_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProductCategories"
    ADD CONSTRAINT "FK_ProductCategories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("ProductId") ON DELETE CASCADE;


--
-- Name: Products FK_Products_Providers_ProviderId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "FK_Products_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES public."Providers"("ProviderId");


--
-- Name: ProviderAssets FK_ProviderAssets_Assets_AssetId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProviderAssets"
    ADD CONSTRAINT "FK_ProviderAssets_Assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES public."Assets"("AssetId") ON DELETE CASCADE;


--
-- Name: ProviderAssets FK_ProviderAssets_Providers_ProviderId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProviderAssets"
    ADD CONSTRAINT "FK_ProviderAssets_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES public."Providers"("ProviderId") ON DELETE CASCADE;


--
-- Name: ReturnOutboundDetails FK_ReturnOutboundDetails_InboundDetails_InboundDetailsId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReturnOutboundDetails"
    ADD CONSTRAINT "FK_ReturnOutboundDetails_InboundDetails_InboundDetailsId" FOREIGN KEY ("InboundDetailsId") REFERENCES public."InboundDetails"("InboundDetailsId");


--
-- Name: ReturnOutboundDetails FK_ReturnOutboundDetails_OutboundDetails_OutboundDetailsId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReturnOutboundDetails"
    ADD CONSTRAINT "FK_ReturnOutboundDetails_OutboundDetails_OutboundDetailsId" FOREIGN KEY ("OutboundDetailsId") REFERENCES public."OutboundDetails"("OutboundDetailsId") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


DROP TRIGGER IF EXISTS inventory_transaction_trigger ON "Lots";

DROP FUNCTION IF EXISTS log_stock_transaction();

DO $outer$
BEGIN
     -- Define the trigger function
        CREATE OR REPLACE FUNCTION log_stock_transaction()
        RETURNS TRIGGER AS $inner$
        BEGIN
            INSERT INTO "InventoryTransactions" ("LotId", "Quantity", "CreatedAt")
            VALUES (NEW."LotId", NEW."Quantity", now());
            RETURN NEW;
        END;
        $inner$ LANGUAGE plpgsql;

        -- Attach the trigger
        CREATE TRIGGER inventory_transaction_trigger
        AFTER INSERT OR UPDATE ON "Lots"
        FOR EACH ROW EXECUTE FUNCTION log_stock_transaction();
END;
$outer$;

CREATE TABLE public.airports (
	code bpchar(3) NOT NULL,
	"name" varchar(256) NOT NULL,
	city_name varchar(256) NOT NULL,
	CONSTRAINT airports_pk PRIMARY KEY (code)
);

CREATE TABLE public.flights (
	pk int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	flight_number varchar(4) NULL,
	airline bpchar(2) NULL,
	scheduled timestamp NULL,
	estimated timestamp NULL,
	actual timestamp NULL,
	airport_gate varchar(4) NULL,
	airport_code bpchar(3) NULL,
	created timestamp DEFAULT (now() AT TIME ZONE 'utc'::text) NOT NULL,
	status varchar(256) NULL,
	disposition bool NOT NULL,
	stale bool NOT NULL,
	codeshares bool NOT NULL,
	CONSTRAINT flights_unique UNIQUE (pk)
);
CREATE INDEX flights_scheduled_idx ON public.flights USING btree (scheduled);
ALTER TABLE public.flights ADD CONSTRAINT flights_airports_fk FOREIGN KEY (airport_code) REFERENCES public.airports(code);

CREATE TABLE public.flight_codeshare_partners (
	pk int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	flight_pk int4 NOT NULL,
	codeshare_id varchar(8) NOT NULL
);
ALTER TABLE public.flight_codeshare_partners ADD CONSTRAINT flight_codeshare_partners_flights_fk FOREIGN KEY (flight_pk) REFERENCES public.flights(pk);

ALTER TABLE public.flights ADD aircraft_type varchar(10) NULL;

CREATE TABLE public.failed_flights (
	pk bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
	attempt_timestamp timestamp NOT NULL,
	raw_flight_data text NOT NULL,
	error text NOT NULL,
	fixed bool NOT NULL
);

CREATE TABLE public.airline_flight_pax (
	airline bpchar(2) NOT NULL,
	aircraft_type varchar(10) NOT NULL,
	pax_count int DEFAULT 0 NOT NULL,
	CONSTRAINT airline_flight_pax_pk PRIMARY KEY (airline,aircraft_type)
);

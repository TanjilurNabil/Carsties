"use client";
import { useParamStore } from "@/hooks/useParamStore";
import { Auction, PagedResult } from "@/types";
import qs from "query-string";
import { useEffect, useState } from "react";
import { useShallow } from "zustand/shallow";
import { getData } from "../actions/auctionActions";
import AppPagination from "../components/AppPagination";
import AuctionCard from "./AuctionCard";
import Filters from "./Filters";
export default function Listings() {
  const [data, setData] = useState<PagedResult<Auction>>();
  const params = useParamStore(
    useShallow((state) => ({
      pageNumber: state.pageNumber,
      pageSize: state.pageSize,
      searchTerm: state.searchTerm,
    }))
  );
  const setParams = useParamStore((state) => state.setParams);
  const url = qs.stringifyUrl({ url: "", query: params });

  function setPageNumber(pageNumber: number) {
    setParams({ pageNumber });
  }
  useEffect(() => {
    getData(url).then((data) => {
      setData(data);
    });
  }, [url]);
  if (!data) return <h3>Loading...</h3>;
  return (
    <>
      <Filters />
      <div className="grid grid-cols-4 gap-6">
        {data.results.map((auction) => (
          <AuctionCard auction={auction} key={auction.id} />
        ))}
      </div>
      <div className="flex justify-center mt-4">
        <AppPagination
          currentPage={params.pageNumber}
          pageCount={data.pageCount}
          pageChanged={setPageNumber}
        />
      </div>
    </>
  );
}

import { create } from "zustand";

type State = {
  pageSize: number;
  pageNumber: number;
  pageCount: number;
  searchTerm: string;
};

type Actions = {
  setParams: (params: Partial<State>) => void;
  reset: () => void;
};

const initialState: State = {
  pageSize: 12,
  pageNumber: 1,
  pageCount: 1,
  searchTerm: "",
};
export const useParamStore = create<State & Actions>()((set) => ({
  ...initialState,

  setParams: (newParams: Partial<State>) => {
    set((state) => {
      if (newParams.pageNumber) {
        return { ...state, pageNumber: newParams.pageNumber };
      } else {
        return { ...state, ...newParams, pageNumber: 1 };
      }
    });
  },
  reset: () => set(initialState),
}));

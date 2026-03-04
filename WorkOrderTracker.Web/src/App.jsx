import "./App.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import WorkOrdersListPage from "./WorkOrdersListPage";
import WorkOrderPage from "./WorkOrderPage";
import NotePage from "./NotePage";
import CreateWorkOrderPage from "./CreateWorkOrderPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<WorkOrdersListPage />} />
        <Route path="/notes/:id" element={<WorkOrderPage />} />
        <Route path="/note/:id" element={<NotePage />} />
        <Route path="/createWorkOrder" element={<CreateWorkOrderPage />} />
      </Routes>
    </BrowserRouter>
  );
}

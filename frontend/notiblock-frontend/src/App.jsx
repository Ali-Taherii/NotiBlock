import AppRouter from "./router/index.jsx";
import { AuthProvider } from "./contexts/AuthProvider.jsx";
import "./styles/App.css";

export default function App() {
  return (
    <AuthProvider>
      <AppRouter />
    </AuthProvider>
  );
}

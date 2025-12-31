import { useTheme } from "../ThemeContext.jsx";
import "./DarkModeButton.css";
import MoonIcon from "../images/moon.svg?react";

function DarkModeButton() {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="dark-mode-button-container">
      <button
        className="toggle-dark-mode"
        onClick={toggleTheme}
        aria-label="Toggle dark mode"
        aria-pressed={theme === "dark"}
      >
        <MoonIcon className="icon" />
      </button>
    </div>
  );
}

export default DarkModeButton;

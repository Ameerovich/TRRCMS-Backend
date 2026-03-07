"""
Login dialog for TRRCMS server authentication.
"""

from qgis.PyQt.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QLabel, QLineEdit,
    QPushButton, QMessageBox, QFormLayout
)
from qgis.PyQt.QtCore import Qt

from .api_client import TRRCMSApiClient


class LoginDialog(QDialog):
    """Dialog for logging in to the TRRCMS server."""

    def __init__(self, parent=None, api_client=None):
        super().__init__(parent)
        self.api_client = api_client
        self.setWindowTitle("TRRCMS Login")
        self.setMinimumWidth(400)
        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self._setup_ui()

    def _setup_ui(self):
        layout = QVBoxLayout(self)

        # Title
        title = QLabel("TRRCMS - Building Registration")
        title.setStyleSheet("font-size: 14px; font-weight: bold; margin-bottom: 10px;")
        title.setAlignment(Qt.AlignCenter)
        layout.addWidget(title)

        # Form
        form = QFormLayout()

        self.server_url_input = QLineEdit("http://localhost:8080")
        self.server_url_input.setPlaceholderText("http://localhost:8080")
        form.addRow("Server URL:", self.server_url_input)

        self.username_input = QLineEdit()
        self.username_input.setPlaceholderText("Enter username")
        form.addRow("Username:", self.username_input)

        self.password_input = QLineEdit()
        self.password_input.setPlaceholderText("Enter password")
        self.password_input.setEchoMode(QLineEdit.Password)
        form.addRow("Password:", self.password_input)

        layout.addLayout(form)

        # Status label
        self.status_label = QLabel("")
        self.status_label.setAlignment(Qt.AlignCenter)
        self.status_label.setWordWrap(True)
        layout.addWidget(self.status_label)

        # Buttons
        btn_layout = QHBoxLayout()
        self.login_btn = QPushButton("Login")
        self.login_btn.setDefault(True)
        self.login_btn.clicked.connect(self._on_login)
        btn_layout.addWidget(self.login_btn)

        cancel_btn = QPushButton("Cancel")
        cancel_btn.clicked.connect(self.reject)
        btn_layout.addWidget(cancel_btn)

        layout.addLayout(btn_layout)

    def _on_login(self):
        server_url = self.server_url_input.text().strip()
        username = self.username_input.text().strip()
        password = self.password_input.text()

        if not server_url:
            self.status_label.setText("Please enter the server URL.")
            self.status_label.setStyleSheet("color: red;")
            return

        if not username or not password:
            self.status_label.setText("Please enter username and password.")
            self.status_label.setStyleSheet("color: red;")
            return

        self.login_btn.setEnabled(False)
        self.status_label.setText("Logging in...")
        self.status_label.setStyleSheet("color: gray;")

        try:
            # Create or update api_client with the server URL
            if self.api_client is None:
                self.api_client = TRRCMSApiClient(server_url)
            else:
                self.api_client.base_url = server_url.rstrip("/")

            self.api_client.login(username, password)

            self.status_label.setText("Login successful!")
            self.status_label.setStyleSheet("color: green;")
            self.accept()

        except Exception as e:
            self.status_label.setText(str(e))
            self.status_label.setStyleSheet("color: red;")
            self.login_btn.setEnabled(True)

    def get_api_client(self):
        """Return the authenticated API client."""
        return self.api_client

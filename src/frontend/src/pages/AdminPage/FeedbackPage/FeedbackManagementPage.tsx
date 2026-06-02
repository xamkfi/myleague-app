import { useTranslation } from "react-i18next";
import AdminPageTemplate from "../../../components/PageTemplate/AdminPageTemplate";
import FeedbackList from "./components/FeedbackList";

const FeedbackManagementPage = () => {
    const { t } = useTranslation();

    return(
        <AdminPageTemplate title={t('admin.feedback.pageTitle', 'Feedback Management')}>
            <div>
                <h1>Manage Feedback</h1>
                <FeedbackList/>
            </div>
        </AdminPageTemplate>
    );
};

export default FeedbackManagementPage;
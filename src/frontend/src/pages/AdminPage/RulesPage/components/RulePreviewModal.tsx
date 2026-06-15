import type { RuleItem } from "../../../../types/admin/ruleTypes";
import "./RulePreviewModal.scss";

interface RulePreviewModalProps {
    rule: RuleItem | null;
    onClose: () => void;
}

export default function RulePreviewModal({
    rule,
    onClose,
}: Readonly<RulePreviewModalProps>) {
    if (!rule) {
        return null;
    }

    return (
        <div className="rule-preview-modal" role="dialog" aria-modal="true">
            <button
                type="button"
                className="rule-preview-modal__backdrop"
                onClick={onClose}
                aria-label="Sulje esikatselu"
            />

            <div className="rule-preview-modal__content">
                <div className="rule-preview-modal__header">
                    <div>
                        <span className="rule-preview-modal__badge">
                            Esikatselu
                        </span>
                        <h2>Säännön esikatselu</h2>
                    </div>

                    <button
                        type="button"
                        className="rule-preview-modal__close"
                        onClick={onClose}
                        aria-label="Sulje esikatselu"
                    >
                        ×
                    </button>
                </div>

                <div className="rule-preview-modal__body">
                    <div className="rule-preview-modal__rule-card">
                        <div className="rule-preview-modal__number">01.</div>

                        <div
                            className="rule-preview-modal__html"
                            dangerouslySetInnerHTML={{ __html: rule.html }}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}

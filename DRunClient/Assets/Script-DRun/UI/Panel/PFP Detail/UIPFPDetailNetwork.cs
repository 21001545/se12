using DRun.Client.Logic.ProMode;
using Festa.Client;
using System;
using DRun.Client.NetData;
using DRun.Client.RefData;
using Festa.Client.Module;
using Festa.Client.RefData;
using Void = Festa.Client.Module.Void;

namespace DRun.Client
{
    public static class UIPFPDetailNetwork
    {
        public static void rechargeNFTStamina(
            ClientNFTItem nftItem,
            int amount,
            Action<AsyncResult<Void>> onSucceed = null,
            Action<AsyncResult<Void>> onFail = null)
        {
            RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(nftItem.grade);

            // 스태미나는 내부적으로 100곱해서 사용 (소수점 2자리 처리 위해)
            int max_stamina = refGrade.stamina * 100;
            // amount *= 100;

            if (nftItem.stamina + amount > max_stamina)
            {
                amount = max_stamina - nftItem.stamina;
            }

            RefillNFTStaminaProcessor.create(nftItem.token_id, amount)
                .run(handler =>
                {
                    if (handler.succeeded())
                    {
                        onSucceed?.Invoke(handler);
                    }
                    else
                    {
                        onFail?.Invoke(handler);
                    }
                });
        }

        public static void setPFPImage(int tokenId, UIPhotoThumbnail pfpImage)
        {
            ClientMain.instance
                .getNFTMetadataCache()
                .getMetadata(tokenId, cache =>
                    {
                        if (cache == null)
                        {
							pfpImage.setEmpty();
						}
						else
                        {
                            pfpImage.setImageFromCDN(cache.imageUrl);
                        }
                    }
                );
        }

        public static void purchaseExperience(
            ClientNFTItem nftItem,
            int amount,
            Action<AsyncResult<Void>> onSucceed = null,
            Action<AsyncResult<Void>> onFail = null)
        {
            RefNFTLevel refLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(nftItem.level + 1);
            // 만렙이다
            if (refLevel == null)
            {
                return;
            }

            // RefData상의 경험치는 km, 서버는 미터 단위 (소수점 처리때문에)
            int max_exp = refLevel.required_disance * 1000;
            // amount *= 1000;

            if (nftItem.exp + amount > max_exp)
            {
                amount = max_exp - nftItem.exp;
            }

            BoostExpNFTProcessor.create(nftItem.token_id, amount)
                .run(result =>
                {
                    if (result.succeeded())
                    {
                        onSucceed?.Invoke(result);
                    }
                    else
                    {
                        onFail?.Invoke(result);
                    }
                });
        }

        // 하루 2회 제한에 걸리는지 검사
        public static void checkLevelUpLimit(
            int tokenId,
            Action<(bool, int)> onSucceed = null,
            Action<(bool, int, AsyncResult<Void>)> onFail = null)
        {
            var step = CheckNFTLevelUpLimitProcessor.create(tokenId);

            step.run(result =>
            {
                int configLimitCount = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.daily_levelup_limit, 2);
                if (step.getCheckResult())
                {
                    onSucceed?.Invoke((true, configLimitCount));
                }
                else
                {
                    onFail?.Invoke((false, configLimitCount, result));
                }
            });
        }
    }
}